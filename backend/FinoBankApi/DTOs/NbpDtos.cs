namespace FinoBankApi.DTOs
{
    public class NbpTableDto
    {
        public string Table { get; set; }
        public string No { get; set; }
        public string EffectiveDate { get; set; }
        public List<NbpRateDto> Rates { get; set; }
    }

    public class NbpRateDto
    {
        public string Currency { get; set; }
        public string Code { get; set; }
        public decimal Mid { get; set; }
    }

    public class ExchangeRequestDto
    {
        public int FromAccountId { get; set; }
        public int ToAccountId { get; set; }
        public decimal Amount { get; set; }
    }
}