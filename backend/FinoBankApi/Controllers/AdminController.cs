using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinoBankApi.Data;
using FinoBankApi.Services;
using FinoBankApi.Models;

namespace FinoBankApi.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly BankingDbContext _context;
        private readonly ISecurityLogService _securityLog;
        private readonly ITransactionSeederService _transactionSeeder;

        public AdminController(BankingDbContext context, ISecurityLogService securityLog, ITransactionSeederService transactionSeeder) 
        { 
            _context = context; 
            _securityLog = securityLog;
            _transactionSeeder = transactionSeeder;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Where(u => u.Email != "system@finobank.pl") // Lepiej filtrować po emailu/roli niż ID
                .Select(u => new { 
                    u.Id, u.Email, u.FirstName, u.LastName, u.Role, u.IsBlocked, 
                    AccountCount = u.Accounts.Count 
                })
                .ToListAsync();
            
            return Ok(users);
        }

        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserDetails(int id)
        {
            var user = await _context.Users
                .Include(u => u.Accounts).ThenInclude(a => a.Cards) 
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            var result = new {
                user.Id, user.FirstName, user.LastName, user.Email, user.IsBlocked, user.TwoFactorSecret, 
                Accounts = user.Accounts.Select(a => new {
                    a.Id, a.AccountNumber, a.Balance, a.Currency,
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
            await _securityLog.LogAsync(user.Id, "ADMIN_BLOCK_USER", "Blocked by Admin", HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"User {user.Email} has been blocked." });
        }

        [HttpPost("users/{id}/unblock")]
        public async Task<IActionResult> UnblockUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsBlocked = false;
            await _context.SaveChangesAsync();
            await _securityLog.LogAsync(user.Id, "ADMIN_UNBLOCK_USER", "Unblocked by Admin", HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new { message = $"User {user.Email} has been unblocked." });
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetSecurityLogs()
        {
            var logs = await _context.SecurityLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(50)
                .Select(l => new { l.Id, l.Timestamp, l.Action, l.Description, l.IpAddress, l.UserId })
                .ToListAsync();

            return Ok(logs);
        }

        [HttpPost("seed-transactions/{accountId}")]
        public async Task<IActionResult> SeedTransactions(int accountId)
        {
            try
            {
                await _transactionSeeder.SeedTransactionsAsync(accountId);
                return Ok(new { message = "Pomyślnie wygenerowano historię transakcji." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}