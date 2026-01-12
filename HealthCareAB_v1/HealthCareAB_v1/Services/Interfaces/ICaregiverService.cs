using HealthCareAB_v1.Models.DTOs.Caregiver;

namespace HealthCareAB_v1.Services.Interfaces;

public interface ICaregiverService
{
    Task<IEnumerable<CaregiverDto>> GetAllCaregiversAsync();
    Task<CaregiverDto> GetCaregiverByIdAsync(int id);
}
