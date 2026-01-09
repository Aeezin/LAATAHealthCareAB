

using HealthCareAB_v1.Models.Entities;

namespace HealthCareAB_v1.Repositories.Implementations;

public class CaregiverScheduleRepository
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
}