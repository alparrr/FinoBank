using System.ComponentModel.DataAnnotations;

namespace FinoBankApi.DTOs
{
    public class UpdateProfileDto
    {
        [Required]
        public string City { get; set; }
        
        [Required]
        public string Street { get; set; }
        
        [Required]
        [RegularExpression(@"^\d{2}-\d{3}$")]
        public string ZipCode { get; set; }
        
    }
}