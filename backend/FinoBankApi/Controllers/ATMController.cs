using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinoBankApi.Data;
using FinoBankApi.Models;

namespace FinoBankApi.Controllers
{
    [ApiController]
    [Route("api/atm")]
    public class AtmController : ControllerBase
    {
        private readonly BankingDbContext _context;

        public AtmController(BankingDbContext context)
        {
            _context = context;
        }

        public class AtmDepositRequest
        {
            public string AccountNumber { get; set; }
            public decimal Amount { get; set; }
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] AtmDepositRequest request)
        {
            if (request.Amount <= 0) return BadRequest("Invalid amount");

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