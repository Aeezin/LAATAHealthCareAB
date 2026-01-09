using System;
using System.ComponentModel.DataAnnotations;

namespace HealthCareAB_v1.Configuration
{
    /// <summary>
    /// Strongly-typed JWT configuration settings.
    /// Bound from appsettings.json "JwtSettings" section.
    /// </summary>
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        [Range(1, 10080, ErrorMessage = "ExpiryInMinutes must be between 1 and 10080 (1 week)")]
        public int ExpiryInMinutes { get; set; } = 60;
    }
}
