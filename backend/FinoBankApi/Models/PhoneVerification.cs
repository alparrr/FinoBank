using System.ComponentModel.DataAnnotations;

namespace FinoBankApi.Models
{
    public class PhoneVerification
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(9)]
        public string PhoneNumber { get; set; }
        
        [Required]
        [MaxLength(6)]
        public string Code { get; set; } 
        
        public DateTime ExpiryDate { get; set; }
        
        public bool IsUsed { get; set; } = false;
    }
}