namespace HealthCareAB_v1.Models.Entities;

public class Caregiver
{
    public int Id { get; set; }

    public required int UserId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Specialisation { get; set; }
    public required string Room { get; set; }

    public string? Bio { get; set; }

    public bool Verified { get; set; } = false;
    public bool IsAcceptingPatients { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public ICollection<CaregiverSchedule> Schedules { get; set; } = new List<CaregiverSchedule>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}