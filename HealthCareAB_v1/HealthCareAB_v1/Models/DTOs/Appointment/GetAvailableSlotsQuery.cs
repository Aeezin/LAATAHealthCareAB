using System.ComponentModel.DataAnnotations;

namespace HealthCareAB_v1.Models.DTOs;

public class GetAvailableSlotsQuery
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "CaregiverId must be greater than 0")]
    public int CaregiverId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}