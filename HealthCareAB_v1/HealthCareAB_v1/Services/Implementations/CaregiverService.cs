using HealthCareAB_v1.Exceptions;
using HealthCareAB_v1.Models.DTOs.Caregiver;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services.Interfaces;

namespace HealthCareAB_v1.Services.Implementations;

public class CaregiverService : ICaregiverService
{
    private readonly ICaregiverRepository _caregiverRepository;

    public CaregiverService(ICaregiverRepository caregiverRepository)
    {
        _caregiverRepository = caregiverRepository;
    }

    public async Task<IEnumerable<CaregiverDto>> GetAllCaregiversAsync()
    {
        var caregivers = await _caregiverRepository.GetAllAsync();
        return caregivers.Select(MapToDto);
    }

    public async Task<CaregiverDto> GetCaregiverByIdAsync(int id)
    {
        // Validate ID
        if (id <= 0)
        {
            throw new CaregiverValidationException("Id must be greater than 0");
        }

        var caregiver = await _caregiverRepository.GetByIdAsync(id);

        if (caregiver == null)
        {
            throw new CaregiverNotFoundException($"Caregiver with id {id} was not found.");
        }

        return MapToDto(caregiver);
    }

    // Maps Caregiver entity to DTO
    private static CaregiverDto MapToDto(Caregiver caregiver)
    {
        return new CaregiverDto
        {
            Id = caregiver.Id,
            FirstName = caregiver.FirstName,
            LastName = caregiver.LastName,
            Specialisation = caregiver.Specialisation,
            Room = caregiver.Room,
            Bio = caregiver.Bio,
            Verified = caregiver.Verified,
            IsAcceptingPatients = caregiver.IsAcceptingPatients,
        };
    }
}
