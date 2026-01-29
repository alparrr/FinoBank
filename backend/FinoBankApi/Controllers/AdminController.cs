using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinoBankApi.Data;
using FinoBankApi.Services;

namespace FinoBankApi.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly BankingDbContext _context;
        private readonly ISecurityLogService _securityLog;

        public AdminController(BankingDbContext context, ISecurityLogService securityLog) 
        { 
            _context = context; 
            _securityLog = securityLog;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new { 
                    u.Id, 
                    u.Email, 
                    u.FirstName, 
                    u.LastName, 
                    u.Role, 
                    u.IsBlocked, 
                    AccountCount = u.Accounts.Count 
                })
                .ToListAsync();
            
            return Ok(users);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserDetails(int id)
        {
            var user = await _context.Users
                .Include(u => u.Accounts)
                .ThenInclude(a => a.Cards) 
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            var result = new {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.IsBlocked,
                user.TwoFactorSecret, 
                Accounts = user.Accounts.Select(a => new {
                    a.Id,
                    a.AccountNumber,
                    a.Balance,
                    a.Currency,
                    Cards = a.Cards?.Select(c => new { c.CardNumber, c.IsActive })
                })
            };

            return Ok(result);
        }

        [HttpPost("users/{id}/block")]
        public async Task<IActionResult> BlockUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsBlocked = true;
            await _context.SaveChangesAsync();

            await _securityLog.LogAsync(user.Id, "ADMIN_BLOCK_USER", "Account blocked by Administrator", HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"User {user.Email} has been blocked." });
        }

        [HttpPost("users/{id}/unblock")]
        public async Task<IActionResult> UnblockUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsBlocked = false;
            await _context.SaveChangesAsync();

            await _securityLog.LogAsync(user.Id, "ADMIN_UNBLOCK_USER", "Account unblocked by Administrator", HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"User {user.Email} has been unblocked." });
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetSecurityLogs()
        {
            var logs = await _context.SecurityLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(50)
                .Select(l => new {
                    l.Id,
                    l.Timestamp,
                    l.Action,
                    l.Description,
                    l.IpAddress,
                    UserId = l.UserId 
                })
                .ToListAsync();

            return Ok(logs);
        }
    }
}