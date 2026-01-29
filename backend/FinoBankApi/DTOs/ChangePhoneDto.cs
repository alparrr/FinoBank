using System.ComponentModel.DataAnnotations;

namespace FinoBankApi.DTOs
{
    public class PhoneDto
    {
        [Required]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "Phone number must be exactly 9 digits")]
        public string PhoneNumber { get; set; }
    }
    
    public class ConfirmPhoneChangeDto
    {
        [Required]
        [RegularExpression(@"^\d{9}$")]
        public string NewPhoneNumber { get; set; }
        
        [Required]
        [MinLength(6)]
        public string Code { get; set; }
    }
}