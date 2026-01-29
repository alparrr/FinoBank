namespace FinoBankApi.Services
{
    public interface ISmsService
    {
        Task SendVerificationCode(string phoneNumber, string code);
    }
}