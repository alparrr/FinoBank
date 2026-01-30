using FinoBankApi.DTOs;
using FinoBankApi.Models;

namespace FinoBankApi.Services
{
    public interface ICardService
    {
        Task<Card> CreateCardForAccount(int accountId, int userId, string pin);
        Task<List<CardDto>> GetUserCards(int userId);
        Task ChangeLimits(int cardId, int userId, decimal daily, decimal monthly);
        Task BlockCard(int cardId, int userId);
    }
}