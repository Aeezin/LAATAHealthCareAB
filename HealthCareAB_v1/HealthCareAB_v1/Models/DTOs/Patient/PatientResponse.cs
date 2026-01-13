using System.ComponentModel.DataAnnotations;

namespace HealthCareAB_v1.Models.DTOs.Patient;

public class PatientResponse
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public required DateOnly DateOfBirth { get; set; }
    public required string PersonalIdentityNumber { get; set; }
}
