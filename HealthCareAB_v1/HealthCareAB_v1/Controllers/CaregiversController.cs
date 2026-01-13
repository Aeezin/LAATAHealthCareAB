using System.Security.Claims;
using HealthCareAB_v1.Constants;
using HealthCareAB_v1.DTOs;
using HealthCareAB_v1.Exceptions;
using HealthCareAB_v1.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareAB_v1.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        }
    }
}
