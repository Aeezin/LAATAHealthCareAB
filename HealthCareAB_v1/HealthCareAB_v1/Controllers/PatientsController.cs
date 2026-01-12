using System.Security.Claims;
using HealthCareAB_v1.Constants;
using HealthCareAB_v1.DTOs;
using HealthCareAB_v1.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareAB_v1.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PatientsController : ControllerBase
{
    private readonly IAuthService _authService;

    public PatientsController(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var (result, token) = await _authService.LoginPatientAsync(
            request.Identifier,
            request.Password
        );

        if (!result.Success || string.IsNullOrEmpty(token))
        {
            return Unauthorized(new { message = result.Message });
        }

        var cookieOptions = _authService.GetJwtCookieOptions();
        HttpContext.Response.Cookies.Append(CookieNames.Jwt, token, cookieOptions);

        return Ok(
            new
            {
                message = result.Message,
                loggedInUser = result.Username,
                roles = result.Roles,
            }
        );
    }
}
