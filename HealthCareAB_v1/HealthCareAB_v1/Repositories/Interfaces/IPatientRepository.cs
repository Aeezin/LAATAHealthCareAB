using HealthCareAB_v1.Models.Entities;

namespace HealthCareAB_v1.Repositories.Interfaces;

public interface IPatientRepository
{
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<Patient>> GetAllAsync();
    Task<Patient?> GetByIdAsync(int id);
    Task<Patient?> GetByPersonalIdentityNumberAsync(string personalIdentityNumber);
}
