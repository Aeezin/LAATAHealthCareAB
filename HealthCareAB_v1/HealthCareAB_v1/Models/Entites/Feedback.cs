namespace HealthCareAB_v1.Models.Entities;

public class Feedback
{
    public int Id { get; set; }

    public required int AppointmentId { get; set; }
    public required int Rating { get; set; }

    public string? Comment { get; set; }
    public FeedbackStatus Status { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdateAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Appointment Appointment { get; set; } = null!;
}