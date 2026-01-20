using HealthCareAB_v1.Models.Enums;

namespace HealthCareAB_v1.Models.DTOs;

public class CancelAppointmentResponse
{
    public int Id { get; set; }
    public AppointmentStatus Status { get; set; }
    public DateTime? CancelledAt { get; set; }
}
