using HealthCareAB_v1.Configuration;
using HealthCareAB_v1.DTOs;
using HealthCareAB_v1.Models;
using HealthCareAB_v1.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace HealthCareAB_v1.Services
{
    /// <summary>
    /// Service handling authentication operations including registration and login.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly bool _isDevelopment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(
            IUserService userService,
            IJwtTokenService jwtTokenService,
            IOptions<JwtSettings> jwtSettings,
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _jwtTokenService =
                jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            _jwtSettings =
                jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
            _isDevelopment = environment?.IsDevelopment() ?? false;
            _httpContextAccessor =
                httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <inheritdoc />
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            ArgumentNullException.ThrowIfNull(registerDto);

            if (await _userService.ExistsByUsernameAsync(registerDto.Username))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Username is already taken",
                };
            }

            // Determine roles with security check
            var roles = DetermineUserRoles(registerDto.Roles);

            var user = new User
            {
                Username = registerDto.Username,
                PasswordHash = _userService.HashPassword(registerDto.Password),
                Roles = roles,
            };

            await _userService.CreateUserAsync(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "User registered successfully",
                Username = user.Username,
                Roles = user.Roles,
            };
        }

        /// <summary>
        /// Determines the roles for a new user.
        /// Default role is set to user.
        /// </summary>
        private List<string> DetermineUserRoles(List<string>? requestedRoles)
        {
            // If no roles requested, default to User
            if (requestedRoles == null || !requestedRoles.Any())
            {
                return new List<string> { Roles.User };
            }

            // Return requested roles (original behavior)
            return requestedRoles;
        }

        /// <inheritdoc />
        public async Task<(AuthResponseDto response, string? token)> LoginAsync(LoginDto loginDto)
        {
            ArgumentNullException.ThrowIfNull(loginDto);

            var user = await _userService.GetUserByUsernameAsync(loginDto.Username);

            if (user == null || !_userService.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return (
                    new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid username or password",
                    },
                    null
                );
            }

            var token = _jwtTokenService.GenerateToken(user);

            return (
                new AuthResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    Username = user.Username,
                    Roles = user.Roles,
                },
                token
            );
        }

        /// <inheritdoc />
        public CookieOptions GetJwtCookieOptions()
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = !_isDevelopment,
                Path = "/",
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
            };
        }

        /// <inheritdoc />
        public CookieOptions GetClearCookieOptions()
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = !_isDevelopment,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(-1),
            };
        }
    }
}
