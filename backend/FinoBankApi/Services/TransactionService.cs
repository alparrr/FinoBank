using Microsoft.EntityFrameworkCore;
using FinoBankApi.Data;
using FinoBankApi.DTOs;
using FinoBankApi.Models;

namespace FinoBankApi.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly BankingDbContext _context;

        public TransactionService(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<TransactionDto> CreateTransaction(CreateTransactionDto createTransactionDto, int userId)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (createTransactionDto.FromAccountId == 0)
                {
                    var dest = await _context.Accounts
                        .FirstOrDefaultAsync(a => a.AccountNumber == createTransactionDto.ToAccountNumber && a.UserId == userId);

                    if (dest == null)
                        throw new Exception("Destination account not found");

                    if (createTransactionDto.Amount <= 0)
                        throw new Exception("Amount must be greater than zero");

                    dest.Balance += createTransactionDto.Amount;

                    var deposit = new Transaction
                    {
                        FromAccountId = null, 
                        ToAccountId = dest.Id,
                        Amount = createTransactionDto.Amount,
                        Currency = dest.Currency,
                        Title = createTransactionDto.Title,
                        Description = createTransactionDto.Description,
                        Status = "Completed",
                        TransactionType = "Deposit"
                    };

                    _context.Transactions.Add(deposit);
                    await _context.SaveChangesAsync();
                    await dbTransaction.CommitAsync();

                    return new TransactionDto
                    {
                        Id = deposit.Id,
                        FromAccountId = deposit.FromAccountId,
                        FromAccountNumber = "EXTERNAL",
                        ToAccountId = deposit.ToAccountId,
                        ToAccountNumber = dest.AccountNumber,
                        Amount = deposit.Amount,
                        Currency = deposit.Currency,
                        Title = deposit.Title,
                        Description = deposit.Description,
                        CreatedAt = deposit.CreatedAt,
                        Status = deposit.Status,
                        TransactionType = deposit.TransactionType
                    };
                }

                //transfer between accounts
                var from = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == createTransactionDto.FromAccountId && a.UserId == userId);

            if (from == null)
                throw new Exception("Source account not found");

            var dest2 = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountNumber == createTransactionDto.ToAccountNumber);

            if (dest2 == null)
                throw new Exception("Destination account not found");

            if (createTransactionDto.Amount <= 0)
                throw new Exception("Amount must be greater than zero");

            if (from.Currency != dest2.Currency)
                throw new Exception("Currency mismatch");

            if (from.Balance < createTransactionDto.Amount)
                throw new Exception("Insufficient funds");

            from.Balance -= createTransactionDto.Amount;
            dest2.Balance += createTransactionDto.Amount;

                var transaction = new Transaction
                {
                    FromAccountId = from.Id,
                    ToAccountId = dest2.Id,
                    Amount = createTransactionDto.Amount,
                    Currency = from.Currency,
                    Title = createTransactionDto.Title,
                    Description = createTransactionDto.Description,
                    Status = "Completed",
                    TransactionType = "Transfer"
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return new TransactionDto
                {
                    Id = transaction.Id,
                    FromAccountId = transaction.FromAccountId,
                    FromAccountNumber = from.AccountNumber,
                    ToAccountId = transaction.ToAccountId,
                    ToAccountNumber = dest2.AccountNumber,
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    Title = transaction.Title,
                    Description = transaction.Description,
                    CreatedAt = transaction.CreatedAt,
                    Status = transaction.Status,
                    TransactionType = transaction.TransactionType
                };
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<TransactionDto>> GetAccountTransactions(int accountId, int userId)
        {
            var accountExists = await _context.Accounts
                .AnyAsync(a => a.Id == accountId && a.UserId == userId);

            if (!accountExists)
            {
                throw new Exception("Account not found");
            }

            var transactions = await _context.Transactions
                .Include(t => t.FromAccount)
                .Include(t => t.ToAccount)
                .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    FromAccountId = t.FromAccountId,
                    FromAccountNumber = t.FromAccount != null ? t.FromAccount.AccountNumber : "EXTERNAL",
                    ToAccountId = t.ToAccountId,
                    ToAccountNumber = t.ToAccount.AccountNumber,
                    Amount = t.Amount,
                    Currency = t.Currency,
                    Title = t.Title,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt,
                    Status = t.Status,
                    TransactionType = t.TransactionType
                })
                .ToListAsync();

            return transactions;
        }

        public async Task<List<TransactionDto>> GetUserTransactions(int userId)
        {
            var userAccountIds = await _context.Accounts
                .Where(a => a.UserId == userId)
                .Select(a => a.Id)
                .ToListAsync();

            var transactions = await _context.Transactions
                .Include(t => t.FromAccount)
                .Include(t => t.ToAccount)
                .Where(t => (t.FromAccountId.HasValue && userAccountIds.Contains(t.FromAccountId.Value)) 
                 || userAccountIds.Contains(t.ToAccountId))
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    FromAccountId = t.FromAccountId,
                    FromAccountNumber = t.FromAccount != null ? t.FromAccount.AccountNumber : "EXTERNAL",
                    ToAccountId = t.ToAccountId,
                    ToAccountNumber = t.ToAccount.AccountNumber,
                    Amount = t.Amount,
                    Currency = t.Currency,
                    Title = t.Title,
                    Description = t.Description,
                    CreatedAt = t.CreatedAt,
                    Status = t.Status,
                    TransactionType = t.TransactionType
                })
                .ToListAsync();

            return transactions;
        }
    }
}