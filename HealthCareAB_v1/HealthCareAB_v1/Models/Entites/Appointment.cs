using System.ComponentModel.DataAnnotations;
using HealthCareAB_v1.Models.Enums;

namespace HealthCareAB_v1.Models.Entities;

public class Appointment
{
    public int Id { get; set; }

    public required int PatientId { get; set; }
    public required int CaregiverId { get; set; }

    public required DateOnly Date { get; set; }
    public required TimeOnly StartTime { get; set; }
    public required TimeOnly EndTime { get; set; }

    public string? PatientNotes { get; set; }
    public string? CaregiverNotes { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public Patient Patient { get; set; } = null!;
    public Caregiver Caregiver { get; set; } = null!;
    public Feedback? Feedback { get; set; }
}