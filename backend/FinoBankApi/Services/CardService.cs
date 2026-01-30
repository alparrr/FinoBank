using Microsoft.EntityFrameworkCore;
using FinoBankApi.Data;
using FinoBankApi.DTOs;
using FinoBankApi.Models;
using FinoBankApi.Helpers;

namespace FinoBankApi.Services
{
    public class CardService : ICardService
    {
        private readonly BankingDbContext _context;

        public CardService(BankingDbContext context)
        {
            _context = context;
        }

        public async Task<Card> CreateCardForAccount(int accountId, int userId, string pin) // <--- Dodano parametr pin
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId);
            if (account == null) throw new Exception("Account not found");

            if (await _context.Cards.AnyAsync(c => c.AccountId == accountId))
                throw new Exception("Card already exists for this account");
            
            if (string.IsNullOrEmpty(pin) || pin.Length != 4 || !pin.All(char.IsDigit))
                throw new Exception("PIN must be exactly 4 digits.");

            var random = new Random();
            var card = new Card
            {
                AccountId = accountId,
                CardNumber = "4" + GenerateRandomDigits(15),
                CVV = GenerateRandomDigits(3),
                PIN = BCrypt.Net.BCrypt.HashPassword(pin),
                ExpiryDate = DateTime.UtcNow.AddYears(3),
                IsActive = true,
                IsBlocked = false,
                DailyLimit = 2000,
                MonthlyLimit = 10000
            };

            _context.Cards.Add(card);
            await _context.SaveChangesAsync();
            return card;
        }

        public async Task<List<CardDto>> GetUserCards(int userId)
        {
            return await _context.Cards
                .Include(c => c.Account)
                .Where(c => c.Account.UserId == userId)
                .Select(c => new CardDto
                {
                    Id = c.Id,
                    CardNumber = c.CardNumber,
                    ExpiryDate = c.ExpiryDate.ToString("MM/yy"),
                    IsBlocked = c.IsBlocked,
                    DailyLimit = c.DailyLimit,
                    MonthlyLimit = c.MonthlyLimit,
                    AccountCurrency = c.Account.Currency
                })
                .ToListAsync();
        }

        public async Task ChangeLimits(int cardId, int userId, decimal daily, decimal monthly)
        {
            var card = await _context.Cards
                .Include(c => c.Account)
                .FirstOrDefaultAsync(c => c.Id == cardId && c.Account.UserId == userId);

            if (card == null) throw new Exception("Card not found");

            card.DailyLimit = daily;
            card.MonthlyLimit = monthly;
            await _context.SaveChangesAsync();
        }

        public async Task BlockCard(int cardId, int userId)
        {
            var card = await _context.Cards
                .Include(c => c.Account)
                .FirstOrDefaultAsync(c => c.Id == cardId && c.Account.UserId == userId);

            if (card == null) throw new Exception("Card not found");

            card.IsBlocked = true;
            await _context.SaveChangesAsync();
        }

        private string GenerateRandomDigits(int length)
        {
            var random = new Random();
            string result = "";
            for (int i = 0; i < length; i++) result += random.Next(0, 10).ToString();
            return result;
        }
    }
}