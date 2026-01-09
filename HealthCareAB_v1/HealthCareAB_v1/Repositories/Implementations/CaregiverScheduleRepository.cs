

using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Repositories.Interfaces;

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

    public async Task<bool> HasOverlappingScheduleAsync(int caregiverId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        return await _context.CaregiverSchedules
            .Where(s => s.CaregiverId == caregiverId
                     && s.DayOfWeek == dayOfWeek
                     && s.IsActive)
            .AnyAsync(s =>
                // Check all three overlap scenarios:
                (startTime >= s.StartTime && startTime < s.EndTime) ||      // New start overlaps existing
                (endTime > s.StartTime && endTime <= s.EndTime) ||          // New end overlaps existing
                (startTime <= s.StartTime && endTime >= s.EndTime));        // New schedule encompasses existing
    }
}