using HealthCareAB_v1.Exceptions;
using HealthCareAB_v1.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareAB_v1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    /// <summary>
    /// Gets all patients
    /// </summary>
    /// <returns>List of all patients</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var patients = await _patientService.GetAllPatientsAsync();
        return Ok(patients);
    }

    /// <summary>
    /// Gets a patient by ID
    /// </summary>
    /// <param name="id">The patient ID</param>
    /// <returns>The patient with the specified ID</returns>
    /// <response code="200">Returns the patient</response>
    /// <response code="404">Patient not found</response>
    /// <response code="400">Invalid ID format</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var patient = await _patientService.GetPatientByIdAsync(id);
            return Ok(patient);
        }
        catch (PatientNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (PatientValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
