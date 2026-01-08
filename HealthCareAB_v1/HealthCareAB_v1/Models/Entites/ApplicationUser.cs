using Microsoft.AspNetCore.Identity;

namespace HealthCareAB_v1.Models.Entities;

public class ApplicationUser : IdentityUser<int>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Patient? Patient { get; set; }
    public Caregiver? Caregiver { get; set; }
}