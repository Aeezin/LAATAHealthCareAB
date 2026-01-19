using System;
using HealthCareAB_v1.DTOs;

namespace HealthCareAB_v1.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterPatientAsync(RegisterDto registerDto);

        Task<AuthResponseDto> RegisterCaregiverAsync(RegisterCaregiverDto registerCaregiverDto);

        Task<(AuthResponseDto response, string? token)> LoginPatientAsync(
            string personalIdentityNumber,
            string password
        );
        Task<(AuthResponseDto response, string? token)> LoginCaregiverAsync(
            string username,
            string password
        );
        CookieOptions GetJwtCookieOptions();
        CookieOptions GetClearCookieOptions();
    }
}
