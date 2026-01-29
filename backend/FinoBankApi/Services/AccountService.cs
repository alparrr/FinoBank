using Microsoft.EntityFrameworkCore;
using FinoBankApi.Data;
using FinoBankApi.DTOs;
using FinoBankApi.Models;
using FinoBankApi.Helpers;

namespace FinoBankApi.Services
{
    public class AccountService : IAccountService
    {
        private readonly BankingDbContext _context;

        public AccountService(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<List<AccountDto>> GetUserAccounts(int userId)
        {
            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId)
                .Select(a => new AccountDto
                {
                    Id = a.Id,
                    AccountNumber = a.AccountNumber,
                    Balance = a.Balance,
                    Currency = a.Currency,
                    AccountType = a.AccountType,
                    CreatedAt = a.CreatedAt,
                    IsActive = a.IsActive
                })
                .ToListAsync();

            return accounts;
        }

        public async Task<AccountDto> GetAccountById(int accountId, int userId)
        {
            var account = await _context.Accounts
                .Where(a => a.Id == accountId && a.UserId == userId)
                .Select(a => new AccountDto
                {
                    Id = a.Id,
                    AccountNumber = a.AccountNumber,
                    Balance = a.Balance,
                    Currency = a.Currency,
                    AccountType = a.AccountType,
                    CreatedAt = a.CreatedAt,
                    IsActive = a.IsActive
                })
                .FirstOrDefaultAsync();

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            return account;
        }

        public async Task<AccountDto> CreateAccount(CreateAccountDto createAccountDto, int userId)
        {
            var account = new Account
            {
                AccountNumber = AccountNumberGenerator.Generate(),
                Balance = 0,
                Currency = createAccountDto.Currency,
                AccountType = createAccountDto.AccountType,
                UserId = userId
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return new AccountDto
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                Currency = account.Currency,
                AccountType = account.AccountType,
                CreatedAt = account.CreatedAt,
                IsActive = account.IsActive
            };
        }

        public async Task<decimal> GetAccountBalance(int accountId, int userId)
        {
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);

            if (account == null)
            {
                throw new Exception("Account not found");
            }

            return account.Balance;
        }
    }
}