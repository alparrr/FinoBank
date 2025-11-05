using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinoBankApi.Models
{
    public class Account
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(26)]
        public string AccountNumber { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; }
        
        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "PLN";
        
        [Required]
        [MaxLength(50)]
        public string AccountType { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }
        
        public ICollection<Transaction> SentTransactions { get; set; }
        public ICollection<Transaction> ReceivedTransactions { get; set; }
    }
}