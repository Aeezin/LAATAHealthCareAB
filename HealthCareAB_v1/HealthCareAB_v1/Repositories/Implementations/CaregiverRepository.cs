using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthCareAB_v1.Repositories.Implementations;

public class CaregiverRepository : ICaregiverRepository
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

    public async Task<Caregiver?> GetByUserIdAsync(int userId)
    {
        return await _context.Caregivers.FirstOrDefaultAsync(c => c.UserId == userId);
    }
}
