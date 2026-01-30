using System.ComponentModel.DataAnnotations;

namespace FinoBankApi.DTOs
{
    public class RegisterDto
    {
        [Required]
        [RegularExpression(@"^[a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ \-]+$", ErrorMessage = "First name can only contain letters")]
        [MinLength(2)]
        public string FirstName { get; set; }
        
        [Required]
        [RegularExpression(@"^[a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ \-]+$", ErrorMessage = "Last name can only contain letters")]
        [MinLength(2)]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
            ErrorMessage = "Password must be at least 8 characters, contain uppercase, lowercase, number and special character")]
        public string Password { get; set; }
        
        [Required]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "PESEL must be exactly 11 digits")]
        public string Pesel { get; set; }

        [Required]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "Phone number must be exactly 9 digits")]
        public string PhoneNumber { get; set; }
        
        [Required]
        [MinLength(6)]
        public string SmsCode { get; set; }
        

        [Required]
        [MinLength(2)]
        [RegularExpression(@"^[a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ0-9 \- \/ \.]+$", ErrorMessage = "Street contains invalid characters")]
        public string Street { get; set; } // Tu pozwalamy na cyfry (np. Złota 44)

        [Required]
        [MinLength(2)]
        [RegularExpression(@"^[a-zA-ZąćęłńóśźżĄĆĘŁŃÓŚŹŻ \-]+$", ErrorMessage = "City can only contain letters")]
        public string City { get; set; }

        [Required]
        [RegularExpression(@"^\d{2}-\d{3}$", ErrorMessage = "ZipCode must be in format XX-XXX")]
        public string ZipCode { get; set; }
    }

    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string Password { get; set; }
        public string? TwoFactorCode { get; set; }
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
        public string Role { get; set; }
    }
}