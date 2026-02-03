using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinoBankApi.Models
{
    public class Contact
    {
        public int Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }
        
        [Required]
        [MaxLength(100)] 
        public string Name { get; set; }
        
        [Required] 
        [MaxLength(28)]
        public string AccountNumber { get; set; }
    }
}