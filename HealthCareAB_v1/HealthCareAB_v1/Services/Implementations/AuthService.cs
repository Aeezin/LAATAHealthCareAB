using HealthCareAB_v1.Configuration;
using HealthCareAB_v1.DTOs;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Repositories.Implementations;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace HealthCareAB_v1.Services
{
    /// <summary>
    /// Service handling authentication operations including registration and login.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly bool _isDevelopment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAppDbContext _dbContext;

        public AuthService(
            IUserService userService,
            IJwtTokenService jwtTokenService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<JwtSettings> jwtSettings,
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor,
            IAppDbContext dbContext
        )
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager =
                signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _jwtTokenService =
                jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            _jwtSettings =
                jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
            _isDevelopment = environment?.IsDevelopment() ?? false;
            _httpContextAccessor =
                httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <inheritdoc />
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            ArgumentNullException.ThrowIfNull(registerDto);

            ApplicationUser? existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email is already taken"
                };
            }

            var transaction = await _dbContext.BeginTransactionAsync();

            try
            {
                ApplicationUser user = new ApplicationUser
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email
                };

                IdentityResult createResult = await _userManager.CreateAsync(user, registerDto.Password);
                if (!createResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = string.Join(", ", createResult.Errors.Select(e => e.Description))
                    };
                }

                IdentityResult roleResult = await _userManager.AddToRoleAsync(user, "Patient");
                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = string.Join(", ", roleResult.Errors.Select(e => e.Description))
                    };
                }

                Patient patient = new Patient
                {
                    UserId = user.Id,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.Lastname,
                    PhoneNumber = registerDto.PhoneNumber,
                    DateOfBirth = registerDto.DateOfBirth,
                    PersonalIdentityNumber = registerDto.PersonalIdentityNumber,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Patients.Add(patient);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "User registered successfully",
                    Username = user.Email,
                    Roles = new List<string> { "Patient" }
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<(AuthResponseDto response, string? token)> LoginAsync(LoginDto loginDto)
        {
            ArgumentNullException.ThrowIfNull(loginDto);

            var user = await _userService.GetUserByEmailAsync(loginDto.Username);

            // if (user == null || !_userService.VerifyPassword(loginDto.Password, user.PasswordHash))
            // {
            //     return (
            //         new AuthResponseDto
            //         {
            //             Success = false,
            //             Message = "Invalid username or password",
            //         },
            //         null
            //     );
            // }

            var token = await _jwtTokenService.GenerateToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            return (
                new AuthResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    Username = user.UserName,
                    Roles = roles.ToList(),
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
