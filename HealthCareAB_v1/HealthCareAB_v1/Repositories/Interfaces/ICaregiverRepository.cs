namespace HealthCareAB_v1.Repositories.Interfaces;

public interface ICaregiverRepository
{
    Task<bool> ExistsAsync(int id);
}