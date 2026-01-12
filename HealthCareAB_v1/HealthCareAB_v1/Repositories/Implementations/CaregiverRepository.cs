using Microsoft.EntityFrameworkCore;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Models.Entities;

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

    public async Task<IEnumerable<Caregiver>> GetAllAsync()
    {
        return await _context.Caregivers.ToListAsync();
    }

    public async Task<Caregiver?> GetByIdAsync(int id)
    {
        return await _context.Caregivers.FirstOrDefaultAsync(c => c.Id == id);
    }
}