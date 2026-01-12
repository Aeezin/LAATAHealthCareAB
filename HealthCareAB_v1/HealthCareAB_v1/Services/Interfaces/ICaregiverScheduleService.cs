
using HealthCareAB_v1.Models.DTOs;
using HealthCareAB_v1.Models.Entities;

namespace HealthCareAB_v1.Services.Interfaces;

public interface ICaregiverScheduleService
{
    Task<CaregiverSchedule> CreateAsync(CaregiverSchedule schedule);
    Task<CaregiverSchedule> GetByIdAsync(int id);
    Task<List<CaregiverSchedule>> GetByCaregiverIdAsync(int caregiverId);
    Task<CaregiverSchedule> UpdateAsync(int id, UpdateCaregiverScheduleRequest req);
}