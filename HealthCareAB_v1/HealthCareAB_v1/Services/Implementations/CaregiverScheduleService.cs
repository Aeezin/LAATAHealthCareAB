using HealthCareAB_v1.Exceptions;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services.Interfaces;

namespace HealthCareAB_v1.Services.Implementations;

public class CaregiverScheduleService : ICaregiverScheduleService
{
    private readonly ICaregiverScheduleRepository _caregiverScheduleRepository;
    // private readonly ICaregiverRepository _caregiverRepository; add to constructor as well

    public CaregiverScheduleService(ICaregiverScheduleRepository caregiverScheduleRepository)
    {
        _caregiverScheduleRepository = caregiverScheduleRepository;
        // _caregiverRepository = caregiverRepository;
    }

    public async Task<CaregiverSchedule> CreateAsync(CaregiverSchedule schedule)
    {
        // Validation 1: Check if Caregiver Exists
        /*
        bool caregiverExists = await _caregiverRepository.ExistsAsync(schedule.CaregiverId);
            if (!caregiverExists)
            {
                throw new CaregiverScheduleNotFoundException($"Caregiver with ID {schedule.CaregiverId} not found");
            }
        */

        // Valdation 2: StartTime < EndTime
        if (schedule.StartTime >= schedule.EndTime)
        {
            throw new CaregiverScheduleValidationException("StartTime must be before EndTime.");
        }

        bool hasOverlap = await _caregiverScheduleRepository.HasOverlappingScheduleAsync(schedule.CaregiverId, schedule.DayOfWeek, schedule.StartTime, schedule.EndTime);

        if (hasOverlap)
        {
            throw new CaregiverScheduleValidationException($"Schedule overlaps with an existing schedule for caregiver {schedule.CaregiverId} on {schedule.DayOfWeek}.");
        }

        return await _caregiverScheduleRepository.CreateAsync(schedule);
    }
}