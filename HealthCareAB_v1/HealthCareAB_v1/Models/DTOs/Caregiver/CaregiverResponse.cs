namespace HealthCareAB_v1.Models.DTOs.Caregiver;

public class CaregiverResponse
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Specialisation { get; set; }
    public required string Room { get; set; }
    public string? Bio { get; set; }
    public bool Verified { get; set; }
    public bool IsAcceptingPatients { get; set; }
}
