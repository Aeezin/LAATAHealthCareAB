
namespace HealthCareAB_v1.Models.DTOs;

public class AvailableTimeSlotsResponse
{
    public int CaregiverId { get; set; }
    public string CaregiverName { get; set; } = string.Empty;
    public List<DailyAvailability> AvailableSlots { get; set; } = new();
}