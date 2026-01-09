using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace HealthCareAB_v1.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Name is required")]
        [RegularExpression(
            @"^[A-Za-zÅÄÖåäö]{1,20}(-[A-Za-zÅÄÖåäö]{1,20})*$",
            ErrorMessage = "Name may contain letters and hyphens only"
        )]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Lastname is required")]
        [RegularExpression(@"^[A-Za-zÅÄÖåäö]{2,20}$")]
        public string Lastname {get; set;} = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [StringLength(
            50,
            MinimumLength = 5)]
        public string Email {get; set;} = null!;

        [Required(ErrorMessage = "Date of Birth is required")]
        [RegularExpression(@"^\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12]\d|3[01])$")]
        public string DateOfBirth {get; set;} = null!;

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(
            @"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$",
            ErrorMessage = "Password must contain at least 1 capital letter, 1 number and 1 special character")]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "Password must be at least 8 characters long")]
        public string Password { get; set; } = null!;

        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number is invalid")]
        public string? PhoneNumber { get; set; }
    }
}
