namespace FinoBankApi.DTOs
{
    public class Setup2faDto
    {
        public string SecretKey { get; set; }
        public string QrCodeSetupImageUrl { get; set; }
        public string ManualEntryKey { get; set; }
    }

    public class Enable2faDto
    {
        public string SecretKey { get; set; }
        public string Code { get; set; }
    }
}