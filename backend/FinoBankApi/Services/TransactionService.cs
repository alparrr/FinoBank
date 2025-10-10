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
                var fromAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Id == createTransactionDto.FromAccountId && a.UserId == userId);

                if (fromAccount == null)
                {
                    throw new Exception("Source account not found");
                }

                var toAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccountNumber == createTransactionDto.ToAccountNumber);

                if (toAccount == null)
                {
                    throw new Exception("Destination account not found");
                }

                if (fromAccount.Balance < createTransactionDto.Amount)
                {
                    throw new Exception("Insufficient funds");
                }

                fromAccount.Balance -= createTransactionDto.Amount;
                toAccount.Balance += createTransactionDto.Amount;

                var transaction = new Transaction
                {
                    FromAccountId = fromAccount.Id,
                    ToAccountId = toAccount.Id,
                    Amount = createTransactionDto.Amount,
                    Currency = fromAccount.Currency,
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
                    FromAccountNumber = t.FromAccount.AccountNumber,
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
                .Where(t => userAccountIds.Contains(t.FromAccountId) || userAccountIds.Contains(t.ToAccountId))
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    FromAccountId = t.FromAccountId,
                    FromAccountNumber = t.FromAccount.AccountNumber,
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