
using HealthCareAB_v1.Models.Entities;

namespace HealthCareAB_v1.Services.Interfaces;

public interface ICaregiverScheduleService
{
    Task<CaregiverSchedule> CreateAsync(CaregiverSchedule schedule);

}