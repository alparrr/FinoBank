using FinoBankApi.Data;
using FinoBankApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinoBankApi.Services
{
    public class TransactionSeederService : ITransactionSeederService
    {
        private readonly BankingDbContext _context;

        public TransactionSeederService(BankingDbContext context)
        {
            _context = context;
        }

        public async Task SeedTransactionsAsync(int accountId)
        {
            var account = await _context.Accounts.FindAsync(accountId);
            if (account == null) throw new Exception("Konto nie zostało znalezione.");

            var systemAccount = await EnsureSystemUserAndAccount();

            var random = new Random();
            var merchants = new[] { "Biedronka", "Lidl", "Orlen", "Netflix", "Uber", "Restauracja Sphinx", "Rossmann", "Steam", "Spotify" };
            var transactions = new List<Transaction>();

            for (int i = 0; i < 15; i++)
            {
                var daysBack = random.Next(1, 60);
                decimal amount = 0;
                string title = "";
                bool isIncome = false;

                if (account.Balance < 100 || random.Next(0, 10) < 1)
                {
                    isIncome = true;
                    amount = random.Next(1800, 2000);
                    title = "Wynagrodzenie";
                }
                else
                {
                    isIncome = false;
                    amount = random.Next(10, 300) + (decimal)random.NextDouble();

                    if (amount > account.Balance)
                    {
                        amount = account.Balance > 1 ? account.Balance - 1 : 0;
                    }
                    title = merchants[random.Next(merchants.Length)];
                }

                amount = Math.Round(amount, 2);

                if (amount > 0)
                {
                    if (isIncome)
                    {
                        transactions.Add(new Transaction
                        {
                            FromAccountId = systemAccount.Id, 
                            ToAccountId = account.Id,
                            Amount = amount,
                            Currency = "PLN",
                            Title = title,
                            Description = "Przelew wynagrodzenia",
                            CreatedAt = DateTime.UtcNow.AddDays(-daysBack),
                            Status = "Completed",
                            TransactionType = "Deposit"
                        });
                        account.Balance += amount;
                    }
                    else
                    {
                        transactions.Add(new Transaction
                        {
                            FromAccountId = account.Id,
                            ToAccountId = systemAccount.Id, 
                            Amount = amount,
                            Currency = "PLN",
                            Title = title,
                            Description = $"Płatność kartą: {title}",
                            CreatedAt = DateTime.UtcNow.AddDays(-daysBack),
                            Status = "Completed",
                            TransactionType = "Transfer"
                        });
                        account.Balance -= amount;
                    }
                }
            }

            if (transactions.Any())
            {
                var sortedTransactions = transactions.OrderBy(t => t.CreatedAt).ToList();
                _context.Transactions.AddRange(sortedTransactions);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<Account> EnsureSystemUserAndAccount()
        {
            var systemUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == "system@finobank.pl");
            
            if (systemUser == null)
            { systemUser = new User
                {
                    Email = "system@finobank.pl",
                    FirstName = "System",
                    LastName = "Rozliczeń",
                    PasswordHash = "SYSTEM_ACCOUNT_NO_LOGIN",
                    Pesel = "00000000000",
                    PhoneNumber = "000000000",
                    City = "Cloud",
                    Street = "Server 1",
                    ZipCode = "00-000",
                    Role = "System",
                    IsBlocked = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Users.Add(systemUser);
                await _context.SaveChangesAsync();
            }

            var systemAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == systemUser.Id);
            if (systemAccount == null)
            {
                systemAccount = new Account
                {
                    Id = 999,
                    AccountNumber = "PL00000000000000000000000000",
                    Balance = 0,
                    Currency = "PLN",
                    AccountType = "External",
                    UserId = systemUser.Id
                };
                _context.Accounts.Add(systemAccount);
                await _context.SaveChangesAsync();
            }

            return systemAccount;
        }
    }
}