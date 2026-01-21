using HealthCareAB_v1.Models.Enums;
namespace HealthCareAB_v1.Models.DTOs;


public class AppointmentResponse
{
    public int Id { get; set; }
    public required int PatientId { get; set; }
    public required int CaregiverId { get; set; }


    public required DateOnly Date { get; set; }
    public required TimeOnly StartTime { get; set; }
    public required TimeOnly EndTime { get; set; }

    public string? PatientNotes { get; set; }
    public string? CaregiverNotes { get; set; }
    public AppointmentStatus Status { get; set; }

    public string? CaregiverName { get; set; }
    public string? CaregiverSpecialisation { get; set; }

    public string? Room { get; set; }

    public string? PatientName { get; set; }
}