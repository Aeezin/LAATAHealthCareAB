using Microsoft.EntityFrameworkCore;

namespace HealthCareAB_v1.Repositories.Implementations;

public class CaregiverRepository
{
    private readonly AppDbContext _context;
    public CaregiverRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Caregivers.AnyAsync(c => c.Id == id);
    }
}