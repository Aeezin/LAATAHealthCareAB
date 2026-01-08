namespace HealthCareAB_v1.Models.Entities;

public class CaregiverSchedule
{
    public int Id { get; set; }

    public required int CaregiverId { get; set; }
    public required DayOfWeek DayOfWeek { get; set; }
    public required TimeOnly StartTime { get; set; }
    public required TimeOnly EndTime { get; set; }

    public bool isActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Caregiver Caregiver { get; set; } = null!;
}