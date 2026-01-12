using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HealthCareAB_v1.Configuration;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HealthCareAB_v1.Services
{
    /// <summary>
    /// Service responsible for generating JWT tokens for authenticated users.
    /// </summary>
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<ApplicationUser> _userManager;

        public JwtTokenService(
            IOptions<JwtSettings> jwtSettings,
            UserManager<ApplicationUser> userManager
        )
        {
            _jwtSettings =
                jwtSettings.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        /// <inheritdoc />
        public async Task<string> GenerateToken(ApplicationUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // Add role claims for authorization
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
