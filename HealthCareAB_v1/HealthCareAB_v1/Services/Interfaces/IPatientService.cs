using HealthCareAB_v1.Models.DTOs.Patient;

namespace HealthCareAB_v1.Services.Interfaces;

public interface IPatientService
{
    Task<IEnumerable<PatientResponse>> GetAllPatientsAsync();
    Task<PatientResponse> GetPatientByIdAsync(int id);
}
