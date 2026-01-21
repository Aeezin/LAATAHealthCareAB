using HealthCareAB_v1.Exceptions;
using HealthCareAB_v1.Models.DTOs.Patient;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services.Interfaces;

namespace HealthCareAB_v1.Services.Implementations;

public class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;

    public PatientService(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository;
    }

    public async Task<IEnumerable<PatientResponse>> GetAllPatientsAsync()
    {
        var patients = await _patientRepository.GetAllAsync();
        return patients.Select(MapToDto);
    }

    public async Task<PatientResponse> GetPatientByIdAsync(int id)
    {
        // Validate ID
        if (id <= 0)
        {
            throw new PatientValidationException("Id must be greater than 0");
        }

        var patient = await _patientRepository.GetByIdAsync(id);

        if (patient == null)
        {
            throw new PatientNotFoundException($"Patient with id {id} was not found.");
        }

        return MapToDto(patient);
    }

    // Maps Patient entity to DTO
    private static PatientResponse MapToDto(Patient patient)
    {
        return new PatientResponse
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Email = patient.User.Email,
            PhoneNumber = patient.PhoneNumber,
            DateOfBirth = patient.DateOfBirth,
            PersonalIdentityNumber = patient.PersonalIdentityNumber,
        };
    }
}
