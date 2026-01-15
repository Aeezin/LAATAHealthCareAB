using HealthCareAB_v1.Models.Entities;

namespace HealthCareAB_v1.Repositories.Interfaces;

public interface IPatientRepository
{
    Task<Patient?> GetByPersonalIdentityNumberAsync(string personalIdentityNumber);
    Task<Patient?> GetByUserIdAsync(int userId);
}
