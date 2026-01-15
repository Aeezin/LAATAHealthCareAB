namespace HealthCareAB_v1.Models.DTOs;

public class UpdateCaregiverScheduleRequest
{
    public DayOfWeek? DayOfWeek { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public bool? IsActive { get; set; }
}