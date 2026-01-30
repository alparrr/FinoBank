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

        public async Task<TransactionDto> CreateTransaction(CreateTransactionDto dto, int userId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var dbTransaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var fromAccount = await _context.Accounts
                        .FirstOrDefaultAsync(a => a.Id == dto.FromAccountId && a.UserId == userId);

                    if (fromAccount == null)
                        throw new Exception("Source account not found or access denied.");

                    var toAccount = await _context.Accounts
                        .FirstOrDefaultAsync(a => a.AccountNumber == dto.ToAccountNumber);

                    if (toAccount == null)
                        throw new Exception("Destination account not found.");

                    if (fromAccount.Id == toAccount.Id)
                        throw new Exception("Cannot transfer money to the same account.");

                    if (dto.Amount <= 0)
                        throw new Exception("Amount must be positive.");

                    if (fromAccount.Currency != toAccount.Currency)
                        throw new Exception("Currency mismatch. Currency exchange not implemented.");

                    if (fromAccount.Balance < dto.Amount)
                        throw new Exception("Insufficient funds.");

                    fromAccount.Balance -= dto.Amount;
                    toAccount.Balance += dto.Amount;

                    var transaction = new Transaction
                    {
                        FromAccountId = fromAccount.Id,
                        ToAccountId = toAccount.Id,
                        Amount = dto.Amount,
                        Currency = fromAccount.Currency,
                        Title = dto.Title,
                        Description = dto.Description,
                        Status = "Completed",
                        TransactionType = "Transfer"
                    };

                    _context.Transactions.Add(transaction);

                    await _context.SaveChangesAsync();
                    await dbTransaction.CommitAsync();

                    return new TransactionDto
                    {
                        Id = transaction.Id,
                        FromAccountId = transaction.FromAccountId ?? 0,
                        FromAccountNumber = fromAccount.AccountNumber,
                        ToAccountId = transaction.ToAccountId,
                        ToAccountNumber = toAccount.AccountNumber,
                        Amount = transaction.Amount,
                        Currency = transaction.Currency,
                        Title = transaction.Title,
                        Description = transaction.Description,
                        CreatedAt = transaction.CreatedAt,
                        Status = transaction.Status,
                        TransactionType = transaction.TransactionType
                    };
                }
                catch (DbUpdateConcurrencyException)
                {
                    await dbTransaction.RollbackAsync();
                    throw new Exception("Transaction failed due to concurrency conflict. Please try again.");
                }
                catch (Exception)
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<List<TransactionDto>> GetAccountTransactions(int accountId, int userId)
        {
             var accountExists = await _context.Accounts.AnyAsync(a => a.Id == accountId && a.UserId == userId);
            if (!accountExists) throw new Exception("Account not found");

            return await _context.Transactions
                .Include(t => t.FromAccount)
                .Include(t => t.ToAccount)
                .Where(t => t.FromAccountId == accountId || t.ToAccountId == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(200)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    FromAccountId = t.FromAccountId ?? 0,
                    FromAccountNumber = t.FromAccount != null ? t.FromAccount.AccountNumber : "ATM",
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
        }

        public async Task<List<TransactionDto>> GetUserTransactions(int userId)
        {
            var userAccountIds = await _context.Accounts
                .Where(a => a.UserId == userId)
                .Select(a => a.Id)
                .ToListAsync();

            return await _context.Transactions
                .Include(t => t.FromAccount)
                .Include(t => t.ToAccount)
                .Where(t => (t.FromAccountId.HasValue && userAccountIds.Contains(t.FromAccountId.Value)) 
                         || userAccountIds.Contains(t.ToAccountId))
                .OrderByDescending(t => t.CreatedAt)
                .Take(20)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    FromAccountId = t.FromAccountId ?? 0,
                    FromAccountNumber = t.FromAccount != null ? t.FromAccount.AccountNumber : "ATM",
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
        }

        public async Task<TransactionDto> GetTransactionById(int transactionId, int userId)
        {
            var transaction = await _context.Transactions
                .Include(t => t.FromAccount)
                        .Include(t => t.ToAccount)
                        .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction == null) return null;

            var userAccountIds = await _context.Accounts
                .Where(a => a.UserId == userId)
                .Select(a => a.Id)
                .ToListAsync();

            if (!userAccountIds.Contains(transaction.FromAccountId ?? 0) && !userAccountIds.Contains(transaction.ToAccountId))
            {
                return null; 
            }

            return new TransactionDto
            {
                Id = transaction.Id,
                FromAccountId = transaction.FromAccountId,
                FromAccountNumber = transaction.FromAccount?.AccountNumber ?? "Wpłata zewnętrzna",
                ToAccountId = transaction.ToAccountId,
                ToAccountNumber = transaction.ToAccount.AccountNumber,
                Amount = transaction.Amount,
                Currency = transaction.Currency,
                Title = transaction.Title,
                Description = transaction.Description,
                CreatedAt = transaction.CreatedAt,
                Status = transaction.Status,
                TransactionType = transaction.TransactionType
            };
        }
    }
}