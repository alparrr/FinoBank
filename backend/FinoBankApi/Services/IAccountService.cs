using FinoBankApi.DTOs;

namespace FinoBankApi.Services
{
    public interface IAccountService
    {
        Task<List<AccountDto>> GetUserAccounts(int userId);
        Task<AccountDto> GetAccountById(int accountId, int userId);
        Task<AccountDto> CreateAccount(CreateAccountDto createAccountDto, int userId);
        Task<decimal> GetAccountBalance(int accountId, int userId);
    }
}