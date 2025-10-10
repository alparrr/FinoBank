using FinoBankApi.DTOs;

namespace FinoBankApi.Services
{
    public interface ITransactionService
    {
        Task<TransactionDto> CreateTransaction(CreateTransactionDto createTransactionDto, int userId);
        Task<List<TransactionDto>> GetAccountTransactions(int accountId, int userId);
        Task<List<TransactionDto>> GetUserTransactions(int userId);
    }
}