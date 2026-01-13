<<<<<<< HEAD
using HealthCareAB_v1.Exceptions;
using HealthCareAB_v1.Services.Interfaces;
=======
using System.Security.Claims;
using HealthCareAB_v1.Constants;
using HealthCareAB_v1.DTOs;
using HealthCareAB_v1.Exceptions;
using HealthCareAB_v1.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
>>>>>>> d9ac91c4ef1d61212b484242f7dcb9b19f967e74
using Microsoft.AspNetCore.Mvc;

namespace HealthCareAB_v1.Controllers;

[ApiController]
[Route("api/[controller]")]
<<<<<<< HEAD
public class CaregiversController : ControllerBase
{
    private readonly ICaregiverService _caregiverService;

    public CaregiversController(ICaregiverService caregiverService)
    {
        _caregiverService = caregiverService;
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
=======
[Produces("application/json")]
public class CaregiverController : ControllerBase
{
    private readonly IAuthService _authService;

    public CaregiverController(IAuthService authService)
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
            var (result, token) = await _authService.LoginCaregiverAsync(
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
>>>>>>> d9ac91c4ef1d61212b484242f7dcb9b19f967e74
        }
    }
}
