using HealthCareAB_v1.Models.Entities;

namespace HealthCareAB_v1.Repositories.Interfaces;

public interface ICaregiverScheduleRepository
{
    Task<CaregiverSchedule> CreateAsync(CaregiverSchedule schedule);
    Task<bool> HasOverlappingScheduleAsync(int caregiverId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime);
}