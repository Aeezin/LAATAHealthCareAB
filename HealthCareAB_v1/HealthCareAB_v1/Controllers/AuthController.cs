using System.Security.Claims;
using HealthCareAB_v1.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HealthCareAB_v1.Constants;
using HealthCareAB_v1.Services.Interfaces;

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
        /// Registers a new user with default User role.
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            var result = await _authService.RegisterAsync(request);

            if (!result.Success)
            {
                return Conflict(new { message = result.Message });
            }

            return CreatedAtAction(
                nameof(CheckAuthentication),
                new { message = result.Message, username = result.Username, roles = result.Roles });
        }

        /// <summary>
        /// Authenticates a user and sets JWT cookie.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var (result, token) = await _authService.LoginAsync(request);

            if (!result.Success || string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = result.Message });
            }

            var cookieOptions = _authService.GetJwtCookieOptions();
            HttpContext.Response.Cookies.Append(CookieNames.Jwt, token, cookieOptions);

            return Ok(new { message = result.Message, username = result.Username, roles = result.Roles });
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
            var roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return Ok(new { message = "Authenticated", username, roles });
        }
    }
}