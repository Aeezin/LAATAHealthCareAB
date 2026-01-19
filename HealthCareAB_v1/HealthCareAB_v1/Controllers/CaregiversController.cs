using HealthCareAB_v1.Constants;
using HealthCareAB_v1.DTOs;
using HealthCareAB_v1.Exceptions;
using HealthCareAB_v1.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareAB_v1.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/[controller]")]
public class CaregiversController : ControllerBase
{
    private readonly ICaregiverService _caregiverService;
    private readonly IAuthService _authService;

    public CaregiversController(ICaregiverService caregiverService, IAuthService authService)
    {
        _caregiverService = caregiverService;
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    /// <summary>
    /// Gets all caregivers
    /// </summary>
    /// <returns>List of all caregivers</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var caregivers = await _caregiverService.GetAllCaregiversAsync();
        return Ok(caregivers);
    }

    /// <summary>
    /// Gets a caregiver by ID
    /// </summary>
    /// <param name="id">The caregiver ID</param>
    /// <returns>The caregiver with the specified ID</returns>
    /// <response code="200">Returns the caregiver</response>
    /// <response code="404">Caregiver not found</response>
    /// <response code="400">Invalid ID format</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var caregiver = await _caregiverService.GetCaregiverByIdAsync(id);
            return Ok(caregiver);
        }
        catch (CaregiverNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (CaregiverValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
