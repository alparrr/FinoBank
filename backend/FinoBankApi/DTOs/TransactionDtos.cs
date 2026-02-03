using System.ComponentModel.DataAnnotations;

namespace FinoBankApi.DTOs
{
    public class CreateTransactionDto
    {
        [Required]
        public int FromAccountId { get; set; }
        
        [Required]
        public string ToAccountNumber { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        public string Description { get; set; }
    }

    public class TransactionDto
    {
        public int Id { get; set; }
        public int? FromAccountId { get; set; }
        public string FromAccountNumber { get; set; }
        public int ToAccountId { get; set; }
        public string ToAccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public string TransactionType { get; set; }
    }

        public class CreateContactDto
        {
            public string Name { get; set; }
            public string AccountNumber { get; set; }
        }
}