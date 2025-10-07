using System.ComponentModel.DataAnnotations;

namespace FinoBankApi.DTOs
{
    public class CreateAccountDto
    {
        [Required]
        public string AccountType { get; set; }
        
        public string Currency { get; set; } = "PLN";
    }

    public class AccountDto
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }
        public string AccountType { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}