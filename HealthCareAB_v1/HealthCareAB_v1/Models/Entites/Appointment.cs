using HealthCareAB_v1.Models.Enums;

namespace HealthCareAB_v1.Models.Entities;

public class Appointment
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int CaregiverId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string? PatientNotes { get; set; }
    public string? CaregiverNotes { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // TODO: Navigation Properties
}