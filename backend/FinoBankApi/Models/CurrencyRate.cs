using System.ComponentModel.DataAnnotations;

namespace FinoBankApi.Models
{
    public class CurrencyRate
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; }
        
        [Required]
        public decimal Rate { get; set; }
        
        public DateTime Date { get; set; }
    }
}