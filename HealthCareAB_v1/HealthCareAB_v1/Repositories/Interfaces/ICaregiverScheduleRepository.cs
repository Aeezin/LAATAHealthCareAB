using HealthCareAB_v1.Models.Entities;

namespace HealthCareAB_v1.Repositories.Interfaces;

public interface ICaregiverScheduleRepository
{
    Task<CaregiverSchedule> CreateAsync(CaregiverSchedule schedule);
    Task<CaregiverSchedule?> GetByIdAsync(int id);
    Task<List<CaregiverSchedule>> GetByCaregiverIdAsync(int caregiverId);
    Task<CaregiverSchedule> UpdateAsync(CaregiverSchedule schedule);

    Task<bool> HasOverlappingScheduleAsync(int caregiverId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime, int? excludeScheduleId = null);
    Task<CaregiverSchedule?> GetScheduleForDayAsync(int caregiverId, DayOfWeek dayOfWeek);
}