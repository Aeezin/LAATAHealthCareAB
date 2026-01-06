using System;
using HealthCareAB_v1.DTOs;

namespace HealthCareAB_v1.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<(AuthResponseDto response, string? token)> LoginAsync(LoginDto loginDto);
        CookieOptions GetJwtCookieOptions();
        CookieOptions GetClearCookieOptions();
    }
}
