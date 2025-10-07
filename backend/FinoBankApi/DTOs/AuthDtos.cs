using System.ComponentModel.DataAnnotations;

namespace FinoBankApi.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string FirstName { get; set; }
        
        [Required]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [MinLength(8)]
        public string Password { get; set; }
        
        [Required]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "PESEL must be 11 digits")]
        public string Pesel { get; set; }
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string Password { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; }
        public UserDto User { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}