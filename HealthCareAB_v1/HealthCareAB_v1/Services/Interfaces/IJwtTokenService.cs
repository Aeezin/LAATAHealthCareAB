using System;
using HealthCareAB_v1.Models;

namespace HealthCareAB_v1.Services.Interfaces
{
    public interface IJwtTokenService
    {
        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        /// <param name="user">The user to generate a token for</param>
        /// <returns>The generated JWT token string</returns>
        string GenerateToken(User user);
    }
}

