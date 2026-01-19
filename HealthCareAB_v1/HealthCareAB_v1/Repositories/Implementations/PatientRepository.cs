using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthCareAB_v1.Repositories.Implementations;

public class PatientRepository : IPatientRepository
{
    private readonly AppDbContext _context;

    public PatientRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Patients.AnyAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Patient>> GetAllAsync()
    {
        return await _context.Patients.ToListAsync();
    }

    public async Task<Patient?> GetByIdAsync(int id)
    {
        return await _context.Patients.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Patient?> GetByPersonalIdentityNumberAsync(string personalIdentityNumber)
    {
        return await _context.Patients.FirstOrDefaultAsync(p =>
            p.PersonalIdentityNumber == personalIdentityNumber
        );
    }

    public async Task<Patient?> GetByUserIdAsync(int userId)
    {
        return await _context.Patients.FirstOrDefaultAsync(c => c.UserId == userId);
    }
}
