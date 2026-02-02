using System.ComponentModel.DataAnnotations;

namespace FinoBankApi.Models
{
    public class Contact
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        
        [Required]
        [MaxLength(100)] 
        public string Name { get; set; }
        
        [Required] 
        [MaxLength(28)]
        public string AccountNumber { get; set; }
    }
}