namespace FinoBankApi.Services
{
    public interface ITransactionSeederService
    {
        Task SeedTransactionsAsync(int accountId);
    }
}