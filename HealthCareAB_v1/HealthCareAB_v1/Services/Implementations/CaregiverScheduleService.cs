using HealthCareAB_v1.Exceptions;
using HealthCareAB_v1.Models.DTOs;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services.Interfaces;

namespace HealthCareAB_v1.Services.Implementations;

public class CaregiverScheduleService : ICaregiverScheduleService
{
    private readonly ICaregiverScheduleRepository _scheduleRepository;
    private readonly ICaregiverRepository _caregiverRepository;

    public CaregiverScheduleService(ICaregiverScheduleRepository scheduleRepository, ICaregiverRepository caregiverRepository)
    {
        _scheduleRepository = scheduleRepository;
        _caregiverRepository = caregiverRepository;
    }

    public async Task<CaregiverSchedule> CreateAsync(CaregiverSchedule schedule)
    {
        // Validation 1: Check if Caregiver Exists
        bool caregiverExists = await _caregiverRepository.ExistsAsync(schedule.CaregiverId);
        if (!caregiverExists)
        {
            throw new CaregiverScheduleNotFoundException($"Caregiver with ID {schedule.CaregiverId} not found.");
        }

        // Validation 2: Weekday only (Monday-Friday)
        if (schedule.DayOfWeek < DayOfWeek.Monday || schedule.DayOfWeek > DayOfWeek.Friday)
        {
            throw new CaregiverScheduleValidationException("Schedules can only be created for weekdays (Monday-Friday).");
        }

        // Valdation 3: StartTime < EndTime
        if (schedule.StartTime >= schedule.EndTime)
        {
            throw new CaregiverScheduleValidationException("StartTime must be before EndTime.");
        }

        // Validation 4: No overlaps
        bool hasOverlap = await _scheduleRepository.HasOverlappingScheduleAsync(schedule.CaregiverId, schedule.DayOfWeek, schedule.StartTime, schedule.EndTime);

        if (hasOverlap)
        {
            throw new CaregiverScheduleValidationException($"Schedule overlaps with an existing schedule for caregiver {schedule.CaregiverId} on {schedule.DayOfWeek}.");
        }

        return await _scheduleRepository.CreateAsync(schedule);
    }

    public async Task<CaregiverSchedule> GetByIdAsync(int id)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(id);

        if (schedule == null)
        {
            throw new CaregiverScheduleNotFoundException($"Schedule with ID {id} not found.");
        }

        return schedule;
    }

    public async Task<List<CaregiverSchedule>> GetByCaregiverIdAsync(int caregiverId)
    {
        return await _scheduleRepository.GetByCaregiverIdAsync(caregiverId);
    }

    public async Task<CaregiverSchedule> UpdateAsync(int id, UpdateCaregiverScheduleRequest req)
    {
        // Validation 1: Check if schedule exists
        var existingSchedule = await GetByIdAsync(id);

        var dayOfWeekToValidate = req.DayOfWeek ?? existingSchedule.DayOfWeek;
        var startTimeToValidate = req.StartTime ?? existingSchedule.StartTime;
        var endTimeToValidate = req.EndTime ?? existingSchedule.EndTime;

        // Validation 2: Weekday only (Monday-Friday) -- if DayOfWeek is being updated
        if (req.DayOfWeek.HasValue)
        {
            if (dayOfWeekToValidate < DayOfWeek.Monday || dayOfWeekToValidate > DayOfWeek.Friday)
            {
                throw new CaregiverScheduleValidationException("Schedules can only be created for weekdays (Monday-Friday).");
            }
        }

        // Validation 3: StartTime < EndTime (if either time is being updated)
        if (req.StartTime.HasValue || req.EndTime.HasValue)
        {
            if (startTimeToValidate >= endTimeToValidate)
            {
                throw new CaregiverScheduleValidationException("StartTime must be before EndTime.");
            }
        }

        // Validation 4: Check for overlaps if day or times are being updated
        if (req.DayOfWeek.HasValue || req.StartTime.HasValue || req.EndTime.HasValue)
        {
            bool hasOverlap = await _scheduleRepository.HasOverlappingScheduleAsync(
                existingSchedule.CaregiverId,
                dayOfWeekToValidate,
                startTimeToValidate,
                endTimeToValidate,
                existingSchedule.Id); // Exclude current schedule from overlap check

            if (hasOverlap)
            {
                throw new CaregiverScheduleValidationException(
                    $"Schedule overlaps with an existing schedule for caregiver {existingSchedule.CaregiverId} on {dayOfWeekToValidate}.");
            }
        }

        // Update only the fields that are provided
        if (req.DayOfWeek.HasValue)
        {
            existingSchedule.DayOfWeek = req.DayOfWeek.Value;
        }

        if (req.StartTime.HasValue)
        {
            existingSchedule.StartTime = req.StartTime.Value;
        }

        if (req.EndTime.HasValue)
        {
            existingSchedule.EndTime = req.EndTime.Value;
        }

        if (req.IsActive.HasValue)
        {
            existingSchedule.IsActive = req.IsActive.Value;
        }

        return await _scheduleRepository.UpdateAsync(existingSchedule);
    }
}