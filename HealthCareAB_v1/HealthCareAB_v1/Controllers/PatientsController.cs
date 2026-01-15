using HealthCareAB_v1.Constants;
using HealthCareAB_v1.DTOs;
using HealthCareAB_v1.Exceptions;
using HealthCareAB_v1.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareAB_v1.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly IAuthService _authService;

    public PatientsController(IPatientService patientService, IAuthService authService)
    {
        _patientService = patientService;
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
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

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        try
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var (result, token) = await _authService.LoginPatientAsync(
                    request.Identifier,
                    request.Password
                );

                if (!result.Success || string.IsNullOrEmpty(token))
                {
                    if (result.IsLockedOut)
                    {
                        return StatusCode(
                            StatusCodes.Status403Forbidden,
                            new { message = result.Message }
                        );
                    }
                    return Unauthorized(new { message = result.Message });
                }

                var cookieOptions = _authService.GetJwtCookieOptions();
                HttpContext.Response.Cookies.Append(CookieNames.Jwt, token, cookieOptions);

                return Ok(
                    new
                    {
                        message = result.Message,
                        loggedInUser = $"{result.FirstName} {result.LastName}",
                        roles = result.Roles,
                    }
                );
            }
            catch (JwtTokenGenerationException ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
