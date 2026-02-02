using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinoBankApi.Models
{
    public class Card
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(16)]
        public string CardNumber { get; set; } 
        
        [Required]
        [MaxLength(100)]
        public string CVV { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string PIN { get; set; } 
        public DateTime ExpiryDate { get; set; }
        
        public bool IsActive { get; set; } = true;
        public bool IsBlocked { get; set; } = false;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal DailyLimit { get; set; } = 1000;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyLimit { get; set; } = 10000;
        
        [ForeignKey("Account")]
        public int AccountId { get; set; }
        public Account Account { get; set; }
    }
}