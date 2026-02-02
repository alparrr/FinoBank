using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using FinoBankApi.Data;
using FinoBankApi.DTOs;
using FinoBankApi.Models;

namespace FinoBankApi.Services
{
    public interface IExchangeService
    {
        Task UpdateRatesFromNbp();
        Task<List<CurrencyRate>> GetCurrentRates();
        Task ExchangeCurrency(int userId, ExchangeRequestDto dto);
    }

    public class ExchangeService : IExchangeService
    {
        private readonly BankingDbContext _context;
        private readonly HttpClient _httpClient;

        public ExchangeService(BankingDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        public async Task UpdateRatesFromNbp()
        {
            var response = await _httpClient.GetAsync("https://api.nbp.pl/api/exchangerates/tables/A?format=json");
            
            if (!response.IsSuccessStatusCode) return;

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var nbpData = JsonSerializer.Deserialize<List<NbpTableDto>>(json, options);

            if (nbpData == null || !nbpData.Any()) return;

            var rates = nbpData.First().Rates;

            _context.CurrencyRates.RemoveRange(_context.CurrencyRates);
            
            foreach (var rate in rates)
            {
                if (new[] { "USD", "EUR", "CHF", "GBP" }.Contains(rate.Code))
                {
                    _context.CurrencyRates.Add(new CurrencyRate
                    {
                        CurrencyCode = rate.Code,
                        Rate = rate.Mid,
                        Date = DateTime.UtcNow
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<List<CurrencyRate>> GetCurrentRates()
        {
            var lastUpdate = await _context.CurrencyRates
            .OrderByDescending(r => r.Date)
            .Select(r => r.Date)
            .FirstOrDefaultAsync();
        
            if (lastUpdate == default || DateTime.UtcNow - lastUpdate > TimeSpan.FromHours(1))
            {
                await UpdateRatesFromNbp();
            }
            
            return await _context.CurrencyRates.ToListAsync();
        }

        public async Task ExchangeCurrency(int userId, ExchangeRequestDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var fromAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == dto.FromAccountId && a.UserId == userId);
                var toAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == dto.ToAccountId && a.UserId == userId);

                if (fromAccount == null || toAccount == null) throw new Exception("Accounts not found");
                if (fromAccount.Balance < dto.Amount) throw new Exception("Insufficient funds");
                
                decimal sourceRateInPln = 1.0m;
                if (fromAccount.Currency != "PLN")
                {
                    var rate = await _context.CurrencyRates.FirstOrDefaultAsync(r => r.CurrencyCode == fromAccount.Currency);
                    if (rate == null) throw new Exception($"No rate for {fromAccount.Currency}");
                    sourceRateInPln = rate.Rate;
                }

                decimal targetRateInPln = 1.0m;
                if (toAccount.Currency != "PLN")
                {
                    var rate = await _context.CurrencyRates.FirstOrDefaultAsync(r => r.CurrencyCode == toAccount.Currency);
                    if (rate == null) throw new Exception($"No rate for {toAccount.Currency}");
                    targetRateInPln = rate.Rate;
                }

                decimal amountInPln = dto.Amount * sourceRateInPln;
                
                decimal finalAmount = amountInPln / targetRateInPln;

                finalAmount = Math.Round(finalAmount, 2);

                fromAccount.Balance -= dto.Amount;
                toAccount.Balance += finalAmount;

                _context.Transactions.Add(new Transaction
                {
                    FromAccountId = fromAccount.Id,
                    ToAccountId = toAccount.Id,
                    Amount = dto.Amount,
                    Currency = fromAccount.Currency,
                    Title = "Wymiana walut",
                    Description = $"Exchange {dto.Amount} {fromAccount.Currency} -> {finalAmount} {toAccount.Currency}",
                    TransactionType = "Exchange",
                    Status = "Completed"
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}