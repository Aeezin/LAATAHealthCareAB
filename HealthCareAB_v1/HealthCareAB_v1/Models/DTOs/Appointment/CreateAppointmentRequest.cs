using System.ComponentModel.DataAnnotations;

namespace HealthCareAB_v1.Models.DTOs;

public class CreateAppointmentRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "PatientId must be greater than 0")]
    public int PatientId { get; init; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "CaregiverId must be greater than 0")]
    public int CaregiverId { get; init; }

    [Required]
    public DateOnly Date { get; init; }

    [Required]
    public TimeOnly StartTime { get; init; }

    [Required]
    public TimeOnly EndTime { get; init; }

    public string? PatientNotes { get; init; }
}