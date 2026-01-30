using System.ComponentModel.DataAnnotations;

namespace FinoBankApi.DTOs
{
    public class CardDto
    {
        public int Id { get; set; }
        public string CardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public bool IsBlocked { get; set; }
        public decimal DailyLimit { get; set; }
        public decimal MonthlyLimit { get; set; }
        public string AccountCurrency { get; set; }
        
        public string CVV { get; set; } 
        public string PIN { get; set; }
    }

    public class CreateCardDto
    {
        public string Pin { get; set; }
    }

    
}