using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthCareAB_v1.Repositories.Implementations;

public class CaregiverScheduleRepository : ICaregiverScheduleRepository
{
    private readonly AppDbContext _context;
    public CaregiverScheduleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CaregiverSchedule> CreateAsync(CaregiverSchedule schedule)
    {
        _context.CaregiverSchedules.Add(schedule);
        await _context.SaveChangesAsync();
        return schedule;
    }

    public async Task<CaregiverSchedule?> GetByIdAsync(int id)
    {
        return await _context.CaregiverSchedules
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<CaregiverSchedule>> GetByCaregiverIdAsync(int caregiverId)
    {
        return await _context.CaregiverSchedules
            .Where(s => s.CaregiverId == caregiverId && s.IsActive)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<CaregiverSchedule> UpdateAsync(CaregiverSchedule schedule)
    {
        _context.CaregiverSchedules.Update(schedule);
        await _context.SaveChangesAsync();
        return schedule;
    }

    public async Task<bool> HasOverlappingScheduleAsync(int caregiverId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime, int? excludeScheduleId = null)
    {
        var query = _context.CaregiverSchedules
            .Where(s => s.CaregiverId == caregiverId
                     && s.DayOfWeek == dayOfWeek
                     && s.IsActive);

        // Exclude the schedule being updated from overlap check
        if (excludeScheduleId.HasValue)
        {
            query = query.Where(s => s.Id != excludeScheduleId.Value);
        }

        return await query.AnyAsync(s =>
            // Check all three overlap scenarios:
            (startTime >= s.StartTime && startTime < s.EndTime) ||      // New start overlaps existing
            (endTime > s.StartTime && endTime <= s.EndTime) ||          // New end overlaps existing
            (startTime <= s.StartTime && endTime >= s.EndTime));        // New schedule encompasses existing
    }

    public async Task<CaregiverSchedule?> GetScheduleForDayAsync(int caregiverId, DayOfWeek dayOfWeek)
    {
        return await _context.CaregiverSchedules
            .Where(s => s.CaregiverId == caregiverId
                     && s.DayOfWeek == dayOfWeek
                     && s.IsActive)
            .FirstOrDefaultAsync();
    }
}