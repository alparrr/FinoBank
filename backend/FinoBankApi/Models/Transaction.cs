using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinoBankApi.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }
        
        [ForeignKey("FromAccount")]
        public int? FromAccountId { get; set; }
        public Account FromAccount { get; set; }
        
        [ForeignKey("ToAccount")]
        public int ToAccountId { get; set; }
        public Account ToAccount { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        [MaxLength(3)]
        public string Currency { get; set; } = "PLN";
        
        [MaxLength(255)]
        public string Title { get; set; }
        
        [MaxLength(500)]
        public string Description { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Completed";
        
        [Required]
        [MaxLength(50)]
        public string TransactionType { get; set; }

    }
}