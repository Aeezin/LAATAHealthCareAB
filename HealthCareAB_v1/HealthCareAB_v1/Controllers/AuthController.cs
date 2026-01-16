using System.Security.Claims;
using HealthCareAB_v1.Constants;
using HealthCareAB_v1.DTOs;
using HealthCareAB_v1.Exceptions;
using HealthCareAB_v1.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareAB_v1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        /// <summary>
        /// Registers a new patient with default User role.
        /// </summary>
        [HttpPost("register-patient")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RegisterPatient([FromBody] RegisterDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterPatientAsync(request);

            if (!result.Success)
            {
                return Conflict(new { message = result.Message });
            }

            return CreatedAtAction(
                nameof(CheckAuthentication),
                new
                {
                    message = result.Message,
                    username = result.Username,
                    roles = result.Roles,
                }
            );
        }

        /// <summary>
        /// Registers a new caregiver with default User role.
        /// </summary>
        [HttpPost("register-caregiver")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> RegisterCaregiver([FromBody] RegisterCaregiverDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterCaregiverAsync(request);

            if (!result.Success)
            {
                return Conflict(new { message = result.Message });
            }

            return CreatedAtAction(
                nameof(CheckAuthentication),
                new
                {
                    message = result.Message,
                    username = result.Username,
                    roles = result.Roles,
                }
            );
        }

        [HttpPost("login-patient")]
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

        [HttpPost("login-caregiver")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LoginCaregiver([FromBody] LoginDto request)
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

        /// <summary>
        /// Logs out the current user by clearing the JWT cookie.
        /// </summary>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            var cookieOptions = _authService.GetClearCookieOptions();
            HttpContext.Response.Cookies.Append(CookieNames.Jwt, string.Empty, cookieOptions);

            return Ok(new { message = "Logged out successfully" });
        }

        /// <summary>
        /// Checks if the current request is authenticated.
        /// </summary>
        [Authorize]
        [HttpGet("check")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult CheckAuthentication()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Unauthorized(new { message = "Not authenticated" });
            }

            var username = User.Identity.Name ?? "Unknown";
            var roles = User
                .Claims.Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return Ok(
                new
                {
                    message = "Authenticated",
                    username,
                    roles,
                }
            );
        }
    }
}
