using System.ComponentModel.DataAnnotations;

namespace HealthCareAB_v1.Models.DTOs;

public class CreateCaregiverScheduleRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "CaregiverId must be greater than 0")]
    public int CaregiverId { get; init; }

    [Required]
    [EnumDataType(typeof(DayOfWeek))] // Case-sensitive
    public DayOfWeek DayOfWeek { get; init; }

    [Required]
    public TimeOnly StartTime { get; init; }

    [Required]
    public TimeOnly EndTime { get; init; }
}