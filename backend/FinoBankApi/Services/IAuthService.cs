using FinoBankApi.DTOs;

namespace FinoBankApi.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> Register(RegisterDto registerDto);
        Task<AuthResponseDto> Login(LoginDto loginDto);
        Task<AuthResponseDto> RefreshTokenFromCookie(string token);
        Task<AuthResponseDto> RefreshToken();

        Setup2faDto GenerateTwoFactorSetup(string email);
        Task EnableTwoFactor(int userId, string secretKey, string code);
        Task UpdateUserProfile(int userId, UpdateProfileDto dto);
        Task SendPhoneVerificationCode(string phoneNumber);
        Task ChangePhoneNumber(int userId, string newPhoneNumber, string code);
        Task SendRegistrationSmsCode(string phoneNumber);
        Task<UserProfileDto> GetUserProfile(int userId);
    }
}