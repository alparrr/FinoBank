namespace FinoBankApi.Services
{
    public class SmsService : ISmsService
    {
        private readonly ILogger<SmsService> _logger;

        public SmsService(ILogger<SmsService> logger)
        {
            _logger = logger;
        }

        public Task SendVerificationCode(string phoneNumber, string code)
        {
            
            _logger.LogInformation("SMS_SENT To:{PhoneNumber} Code:{Code}", phoneNumber, code);
            return Task.CompletedTask;
        }
    }
}