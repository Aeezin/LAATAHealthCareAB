using HealthCareAB_v1.Models.Entities;

namespace HealthCareAB_v1.Repositories.Interfaces;

public interface ICaregiverRepository
{
    Task<bool> ExistsAsync(int id);
    Task<Caregiver?> SaveAsync(Caregiver caregiver);
    Task<IEnumerable<Caregiver>> GetAllAsync();
    Task<Caregiver?> GetByIdAsync(int id);
    Task<Caregiver?> GetByUserIdAsync(int userId);
}
