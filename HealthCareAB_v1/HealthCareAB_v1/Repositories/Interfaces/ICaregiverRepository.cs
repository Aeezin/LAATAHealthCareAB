using HealthCareAB_v1.Models.Entities;

namespace HealthCareAB_v1.Repositories.Interfaces;

public interface ICaregiverRepository
{
    Task<bool> ExistsAsync(int id);
<<<<<<< HEAD
    Task<IEnumerable<Caregiver>> GetAllAsync();
    Task<Caregiver?> GetByIdAsync(int id);
}
=======
    Task<Caregiver?> GetByUserIdAsync(int userId);
}
>>>>>>> d9ac91c4ef1d61212b484242f7dcb9b19f967e74
