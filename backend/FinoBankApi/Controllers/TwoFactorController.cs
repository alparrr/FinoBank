using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FinoBankApi.DTOs;
using FinoBankApi.Services;

namespace FinoBankApi.Controllers
{
    [ApiController]
    [Route("api/2fa")]
    [Authorize]
    public class TwoFactorController : ControllerBase
    {
        private readonly IAuthService _authService;

        public TwoFactorController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("setup")]
        public IActionResult GetSetupInfo()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var setupInfo = _authService.GenerateTwoFactorSetup(email);
            return Ok(setupInfo);
        }

        [HttpPost("enable")]
        public async Task<IActionResult> Enable([FromBody] Enable2faDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            await _authService.EnableTwoFactor(userId, dto.SecretKey, dto.Code);
            return Ok(new { message = "2FA enabled successfully" });
        }
    }
}