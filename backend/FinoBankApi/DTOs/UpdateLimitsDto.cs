using System.ComponentModel.DataAnnotations;

namespace FinoBankApi.DTOs
{

    public class UpdateLimitsDto
        {
            [Required]
            [Range(0, 1000000, ErrorMessage = "Daily limit must be positive")]
            public decimal DailyLimit { get; set; }

            [Required]
            [Range(0, 1000000, ErrorMessage = "Monthly limit must be positive")]
            public decimal MonthlyLimit { get; set; }
        }
}