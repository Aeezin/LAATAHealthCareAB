using System;
using HealthCareAB_v1.Models;

namespace HealthCareAB_v1.Services.Interfaces
{
    public interface IUserService
    {
        Task<bool> ExistsByUsernameAsync(string username);
        Task<User?> GetUserByUsernameAsync(string username);
        Task CreateUserAsync(User user);
    }
}
