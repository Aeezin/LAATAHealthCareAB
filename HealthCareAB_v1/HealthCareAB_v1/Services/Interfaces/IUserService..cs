using System;
using HealthCareAB_v1.Models;
using HealthCareAB_v1.Models.Entities;

namespace HealthCareAB_v1.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> ExistsByUsernameAsync(string username);
        Task<ApplicationUser?> GetUserByUsernameAsync(string username);
        Task CreateUserAsync(ApplicationUser user);
    }
}
