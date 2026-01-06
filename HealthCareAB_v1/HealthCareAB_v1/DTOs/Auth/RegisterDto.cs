using System.ComponentModel.DataAnnotations;

namespace HealthCareAB_v1.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(
            50,
            MinimumLength = 3,
            ErrorMessage = "Username must be between 3 and 50 characters"
        )]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "Password must be at least 6 characters"
        )]
        public string Password { get; set; }

        /// // <summary>
        /// Optional roles for the new user.
        /// Note: Admin role can be assigned manually through Swagger. This is ok in dev, in the future this should
        /// be changed to a more solid sulotion. For now you can leave it as it is if you want.
        /// Non-admin requests with Admin role will be ignored (defaults to User).
        /// </summary>
        public List<string>? Roles { get; set; }
    }
}
