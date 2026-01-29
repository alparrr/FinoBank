using System.ComponentModel.DataAnnotations;

namespace FinoBankApi.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Pesel { get; set; }

        public string? TwoFactorSecret { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public ICollection<Account> Accounts { get; set; }
        public bool IsBlocked { get; set; } = false;

        public string Role { get; set; } = "User";

        [MaxLength(255)]
        public string City { get; set; }
        
        [MaxLength(255)]
        public string Street { get; set; }
        
        [MaxLength(255)]
        public string ZipCode { get; set; }
        [Required]
        [MaxLength(9)]
        public string PhoneNumber { get; set; }
    }
}