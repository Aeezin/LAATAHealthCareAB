using HealthCareAB_v1.Models.Entities;

namespace HealthCareAB_v1.Repositories.Interfaces;

public interface IPatientRepository
{
<<<<<<< HEAD
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<Patient>> GetAllAsync();
    Task<Patient?> GetByIdAsync(int id);
=======
    Task<Patient?> GetByPersonalIdentityNumberAsync(string personalIdentityNumber);
>>>>>>> d9ac91c4ef1d61212b484242f7dcb9b19f967e74
}
