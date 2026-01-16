using System;
using HealthCareAB_v1.Models;
using HealthCareAB_v1.Models.Entities;

namespace HealthCareAB_v1.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> ExistsByEmailAsync(string email);
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        Task CreateUserAsync(ApplicationUser user);
    }
}
