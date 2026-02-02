using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinoBankApi.Data;
using FinoBankApi.Models;
using Microsoft.AspNetCore.Authorization;
using FinoBankApi.Services;

namespace FinoBankApi.Controllers
{
    [ApiController]
    [Route("api/atm")]
    [Authorize(Roles = "Admin,Atm")]
    public class AtmController : ControllerBase
    {
        private readonly BankingDbContext _context;
        private readonly ISecurityLogService _securityLog;

        public AtmController(BankingDbContext context, ISecurityLogService securityLog)
        {
            _context = context;
            _securityLog = securityLog;
        }

        public class AtmDepositRequest
        {
            public string AccountNumber { get; set; }
            public decimal Amount { get; set; }
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] AtmDepositRequest request)
        {
        if (request.Amount <= 0 || request.Amount > 10000) 
            return BadRequest("Invalid amount");
            
            await _securityLog.LogAsync(null, "ATM_DEPOSIT", 
            $"Deposit {request.Amount} to {request.AccountNumber}", 
            HttpContext.Connection.RemoteIpAddress?.ToString());

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == request.AccountNumber);

            if (account == null) return NotFound("Account not found");

            account.Balance += request.Amount;

            var transaction = new Transaction
            {
                FromAccountId = null,
                ToAccountId = account.Id,
                Amount = request.Amount,
                Currency = account.Currency,
                Title = "Wpłata Wpłatomat",
                Description = "Wpłata gotówki w oddziale/ATM",
                TransactionType = "Deposit",
                Status = "Completed"
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deposit successful", newBalance = account.Balance });
        }
    }
}