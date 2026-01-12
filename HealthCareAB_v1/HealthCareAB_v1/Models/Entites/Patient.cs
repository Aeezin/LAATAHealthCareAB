namespace HealthCareAB_v1.Models.Entities;

public class Patient
{
    public int Id { get; set; }

    public required int UserId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }

    public string? PhoneNumber { get; set; }
    public required DateOnly DateOfBirth { get; set; }
    public required string PersonalIdentityNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}