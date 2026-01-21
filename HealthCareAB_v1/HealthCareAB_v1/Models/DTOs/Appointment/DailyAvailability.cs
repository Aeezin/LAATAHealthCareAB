
namespace HealthCareAB_v1.Models.DTOs;

public class DailyAvailability
{
    public DateTime Date { get; set; }
    public List<TimeSlotDto> TimeSlots { get; set; } = new();
}