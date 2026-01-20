using System.ComponentModel.DataAnnotations;

namespace HealthCareAB_v1.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Name is required")]
        [RegularExpression(
            @"^[A-Za-zÅÄÖåäö]{1,20}(-[A-Za-zÅÄÖåäö]{1,20})*$",
            ErrorMessage = "Name may contain letters and hyphens only"
        )]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Lastname is required")]
        [RegularExpression(@"^[A-Za-zÅÄÖåäö]{2,20}$")]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [StringLength(50, MinimumLength = 5)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Personal Identity Number is required.")]
        public string PersonalIdentityNumber { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$")]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "Password must be at least 8 characters long and have at least 1 capital letter, 1 number and 1 special character"
        )]
        public string Password { get; set; } = null!;

        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number is invalid")]
        public string? PhoneNumber { get; set; }
    }
}
