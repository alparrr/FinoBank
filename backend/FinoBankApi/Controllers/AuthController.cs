using Microsoft.AspNetCore.Mvc;
using FinoBankApi.DTOs;
using FinoBankApi.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;

namespace FinoBankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("send-code-register")]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<IActionResult> SendCodeForRegister([FromBody] PhoneDto dto)
        {
            await _authService.SendRegistrationSmsCode(dto.PhoneNumber);
            return Ok(new { message = "Verification code sent." });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var result = await _authService.Register(registerDto);
            SetTokenCookie(result.Token);
            return Ok(new { message = "Registration successful", user = result.User });
        }

        [HttpPost("login")]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var result = await _authService.Login(loginDto);
                SetTokenCookie(result.Token);
                return Ok(new { message = "Login successful", user = result.User });
            }
            catch (Exception ex) when (ex.Message == "2FA_REQUIRED")
            {
                return StatusCode(403, new { message = "2FA code required", requires2FA = true });
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            
            var profile = await _authService.GetUserProfile(userId);
            
            return Ok(profile);
        }

        [HttpPost("change-phone/request")]
        [Authorize]
        public async Task<IActionResult> RequestPhoneChange([FromBody] ConfirmPhoneChangeDto dto)
        {
            await _authService.SendPhoneVerificationCode(dto.NewPhoneNumber);
            return Ok(new { message = "Verification code sent to " + dto.NewPhoneNumber });
        }

        [HttpPost("change-phone/confirm")]
        [Authorize]
        public async Task<IActionResult> ConfirmPhoneChange([FromBody] ConfirmPhoneChangeDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _authService.ChangePhoneNumber(userId, dto.NewPhoneNumber, dto.Code);
            return Ok(new { message = "Phone number updated successfully" });
        }

        [HttpPost("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                
            await _authService.UpdateUserProfile(userId, dto);
                
            return Ok(new { message = "Profile updated successfully" });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("jwt");
            return Ok(new { message = "Logged out" });
        }

        private void SetTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,  
                SameSite = SameSiteMode.None, 
                Expires = DateTime.UtcNow.AddMinutes(60)
            };

            Response.Cookies.Append("jwt", token, cookieOptions);
        }
    }
}