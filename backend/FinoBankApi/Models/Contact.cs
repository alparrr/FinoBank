using System.ComponentModel.DataAnnotations;

namespace FinoBankApi.Models
{
    public class Contact
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [Required] public string Name { get; set; }
        [Required] public string AccountNumber { get; set; }
    }
}