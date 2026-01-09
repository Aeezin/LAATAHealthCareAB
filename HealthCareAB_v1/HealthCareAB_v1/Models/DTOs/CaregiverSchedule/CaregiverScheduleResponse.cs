namespace HealthCareAB_v1.Models.DTOs;

public class CaregiverScheduleResponse
{
    public int Id { get; init; }
    public int CaregiverId { get; init; }
    public DayOfWeek DayOfWeek { get; init; }
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public bool IsActive { get; init; }
}