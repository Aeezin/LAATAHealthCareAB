using System.ComponentModel.DataAnnotations;

namespace HealthCareAB_v1.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Identifier is required")]
        public required string Identifier { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public required string Password { get; set; }
    }
}
