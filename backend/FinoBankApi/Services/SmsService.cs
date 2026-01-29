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
            
            _logger.LogInformation("================================================");
            _logger.LogInformation($"[SMS GATEWAY] To: {phoneNumber} | Code: {code}");
            _logger.LogInformation("================================================");
            
            return Task.CompletedTask;
        }
    }
}