using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Authenticator;
using FinoBankApi.Data;
using FinoBankApi.DTOs;
using FinoBankApi.Models;
using FinoBankApi.Helpers;
using OtpNet;

namespace FinoBankApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly BankingDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ISecurityLogService _securityLog;
        private readonly IHttpContextAccessor _httpContextAccessor; 
        private readonly ISmsService _smsService;
        private readonly EncryptionHelper _encryptionHelper;
        

        public AuthService(
            BankingDbContext context, 
            IConfiguration configuration,
            ISecurityLogService securityLog,
            IHttpContextAccessor httpContextAccessor,
            ISmsService smsService,
            EncryptionHelper encryptionHelper)
        {
            _context = context;
            _configuration = configuration;
            _securityLog = securityLog;
            _httpContextAccessor = httpContextAccessor;
            _smsService = smsService;
            _encryptionHelper = encryptionHelper;
        }

        public async Task<AuthResponseDto> Register(RegisterDto registerDto)
        {
            var verification = await _context.PhoneVerifications
                .Where(v => v.PhoneNumber == registerDto.PhoneNumber && !v.IsUsed)
                .OrderByDescending(v => v.ExpiryDate)
                .FirstOrDefaultAsync();

            if (verification == null) 
                throw new Exception("No verification code found. Please request a code first.");
            
            if (!BCrypt.Net.BCrypt.Verify(registerDto.SmsCode, verification.Code))
            throw new Exception("Invalid SMS code.");
            
            if (verification.ExpiryDate < DateTime.UtcNow) 
                throw new Exception("SMS code expired.");

            verification.IsUsed = true;

            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                throw new Exception("User with this email already exists");

            if (await _context.Users.AnyAsync(u => u.Pesel == registerDto.Pesel))
                throw new Exception("User with this PESEL already exists");

            if (await _context.Users.AnyAsync(u => u.PhoneNumber == registerDto.PhoneNumber))
                throw new Exception("User with this Phone Number already exists");

            var formattedFirstName = TextFormatter.Capitalize(registerDto.FirstName);
            var formattedLastName = TextFormatter.Capitalize(registerDto.LastName);
            var formattedCity = TextFormatter.Capitalize(registerDto.City);
            var formattedStreet = TextFormatter.FormatAddress(registerDto.Street);

            var user = new User
            {
                FirstName = formattedFirstName,
                LastName = formattedLastName,
                Email = registerDto.Email.ToLower(),
                Pesel = _encryptionHelper.Encrypt(registerDto.Pesel),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                TwoFactorSecret = null,
                
                PhoneNumber = registerDto.PhoneNumber,
                City = _encryptionHelper.Encrypt(formattedCity),
                Street = _encryptionHelper.Encrypt(formattedStreet),
                ZipCode = _encryptionHelper.Encrypt(registerDto.ZipCode)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var account = new Account
            {
                AccountNumber = AccountNumberGenerator.Generate(),
                Balance = 1000,
                Currency = "PLN",
                AccountType = "Checking",
                UserId = user.Id
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            await _securityLog.LogAsync(user.Id, "REGISTER", "User registered", GetIpAddress());

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                User = MapToDto(user)
            };
        }

        

        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                await _securityLog.LogAsync(null, "LOGIN_FAILED", $"Invalid credentials for: {loginDto.Email}", GetIpAddress());
                throw new Exception("Invalid email or password");
            }

            if (!string.IsNullOrEmpty(user.TwoFactorSecret))
            {
                if (string.IsNullOrEmpty(loginDto.TwoFactorCode))
                {
                    await _securityLog.LogAsync(user.Id, "LOGIN_2FA_REQUIRED", "2FA code missing", GetIpAddress());
                    throw new Exception("2FA_REQUIRED"); 
                }

                var tfa = new TwoFactorAuthenticator();
                bool isCodeValid = tfa.ValidateTwoFactorPIN(user.TwoFactorSecret, loginDto.TwoFactorCode, false);

                if (!isCodeValid)
                {
                    await _securityLog.LogAsync(user.Id, "LOGIN_2FA_FAILED", "Invalid 2FA code provided", GetIpAddress());
                    throw new Exception("Invalid 2FA Code");
                }
            }

            if (user.IsBlocked)
            {
                await _securityLog.LogAsync(user.Id, "LOGIN_BLOCKED", "Attempt to login to blocked account", GetIpAddress());
                throw new Exception("Account is blocked. Contact support.");
            }

            var userAgent = GetUserAgent();
            await _securityLog.LogAsync(user.Id, "LOGIN_SUCCESS", $"Logged in via {userAgent}", GetIpAddress());

            var token = GenerateJwtToken(user);
            return new AuthResponseDto { Token = token, User = MapToDto(user) };
        }

        public Setup2faDto GenerateTwoFactorSetup(string email)
        {
            var tfa = new TwoFactorAuthenticator();
            
            var key = OtpNet.KeyGeneration.GenerateRandomKey(20);
            var secretKey = OtpNet.Base32Encoding.ToString(key);
            
            var setupInfo = tfa.GenerateSetupCode("FinoBank", email, secretKey, false, 3);

            return new Setup2faDto
            {
                SecretKey = secretKey,
                QrCodeSetupImageUrl = setupInfo.QrCodeSetupImageUrl,
                ManualEntryKey = setupInfo.ManualEntryKey
            };
        }


        public async Task EnableTwoFactor(int userId, string secretKey, string code)
        {
            var tfa = new TwoFactorAuthenticator();
            bool isCorrect = tfa.ValidateTwoFactorPIN(secretKey, code, false);

            if (!isCorrect) throw new Exception("Invalid code. 2FA setup failed.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception("User not found");

            user.TwoFactorSecret = secretKey;
            await _context.SaveChangesAsync();
            
            await _securityLog.LogAsync(user.Id, "2FA_ENABLED", "Two-Factor Authentication enabled", GetIpAddress());
        }
        public async Task SendPhoneVerificationCode(string phoneNumber)
        {
            if (await _context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber))
            {
                throw new Exception("This phone number is already in use.");
            }
            var code = GenerateSecureCode(6);

            var verification = new PhoneVerification
            {
                PhoneNumber = phoneNumber,
                Code = BCrypt.Net.BCrypt.HashPassword(code),
                ExpiryDate = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            };

            _context.PhoneVerifications.Add(verification);
            await _context.SaveChangesAsync();

            await _smsService.SendVerificationCode(phoneNumber, code);
        }

        public async Task ChangePhoneNumber(int userId, string newPhoneNumber, string code)
        {

            var verification = await _context.PhoneVerifications
                .Where(v => v.PhoneNumber == newPhoneNumber && !v.IsUsed)
                .OrderByDescending(v => v.ExpiryDate)
                .FirstOrDefaultAsync();

            if (verification == null) throw new Exception("No verification code found. Request a new one.");
            if (!BCrypt.Net.BCrypt.Verify(code, verification.Code)) throw new Exception("Invalid SMS code.");
            if (verification.ExpiryDate < DateTime.UtcNow) throw new Exception("SMS code expired.");

            var user = await _context.Users.FindAsync(userId);
            user.PhoneNumber = newPhoneNumber;
            
            verification.IsUsed = true;

            await _context.SaveChangesAsync();
            await _securityLog.LogAsync(user.Id, "PHONE_CHANGE", $"Phone changed to {newPhoneNumber}", GetIpAddress());
        }

        public async Task SendRegistrationSmsCode(string phoneNumber)
        {
            if (await _context.Users.AnyAsync(u => u.PhoneNumber == phoneNumber))
            {
                throw new Exception("This phone number is already registered.");
            }

            await SendPhoneVerificationCode(phoneNumber); 
        }

        private string GetIpAddress()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return "Unknown";

            if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
                return context.Request.Headers["X-Forwarded-For"];

            return context.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown";
        }

        private string GetUserAgent()
        {
            var context = _httpContextAccessor.HttpContext;
            return context?.Request.Headers["User-Agent"].ToString() ?? "Unknown";
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiryInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        public async Task<AuthResponseDto> RefreshTokenFromCookie(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var secretKey = jwtSettings["SecretKey"];
                var key = Encoding.UTF8.GetBytes(secretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSettings["Audience"],
                    ValidateLifetime = false, 
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    throw new Exception("Invalid token");

                var userId = int.Parse(userIdClaim);
                var user = await _context.Users.FindAsync(userId);
                
                if (user == null || user.IsBlocked)
                    throw new Exception("User not found or blocked");

                var newToken = GenerateJwtToken(user);
                
                await _securityLog.LogAsync(user.Id, "TOKEN_REFRESH", "JWT token refreshed", GetIpAddress());

                return new AuthResponseDto 
                { 
                    Token = newToken, 
                    User = MapToDto(user) 
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Token refresh failed: " + ex.Message);
            }
        }

        public async Task<AuthResponseDto> RefreshToken()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) 
                throw new Exception("No HTTP context available");

            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new Exception("Invalid token");

            var userId = int.Parse(userIdClaim);
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null || user.IsBlocked)
                throw new Exception("User not found or blocked");

            var newToken = GenerateJwtToken(user);
            
            await _securityLog.LogAsync(user.Id, "TOKEN_REFRESH", "JWT token refreshed", GetIpAddress());

            return new AuthResponseDto 
            { 
                Token = newToken, 
                User = MapToDto(user) 
            };
        }
        
        private UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                Role = user.Role
            };
        }

        public async Task<UserProfileDto> GetUserProfile(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception("User not found");

            return new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Pesel = _encryptionHelper.Decrypt(user.Pesel),
                City = _encryptionHelper.Decrypt(user.City),
                Street = _encryptionHelper.Decrypt(user.Street),
                ZipCode = _encryptionHelper.Decrypt(user.ZipCode),
                CreatedAt = user.CreatedAt,
                Is2faEnabled = !string.IsNullOrEmpty(user.TwoFactorSecret),
                Role = user.Role
            };
        }

        private string GenerateSecureCode(int length)
        {
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var bytes = new byte[length * 4]; 
                rng.GetBytes(bytes);
                var result = new System.Text.StringBuilder(length);
                
                for (int i = 0; i < length; i++)
                {
                    var value = BitConverter.ToUInt32(bytes, i * 4);
                    result.Append(value % 10);
                }
                return result.ToString();
            }
        }

        public async Task UpdateUserProfile(int userId, UpdateProfileDto dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception("User not found");

            user.City = _encryptionHelper.Encrypt(dto.City);
            user.Street = _encryptionHelper.Encrypt(dto.Street);
            user.ZipCode = _encryptionHelper.Encrypt(dto.ZipCode);

            await _context.SaveChangesAsync();

            await _securityLog.LogAsync(user.Id, "PROFILE_UPDATE", "User updated address details", GetIpAddress());
        }
    }
}