using System;
using HealthCareAB_v1.Models;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthCareAB_v1.Services
{
    /// <summary>
    /// Service for user-related operations including CRUD and password management.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IAppDbContext _context;

        public UserService(IAppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(username);
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        /// <inheritdoc />
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(username);
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        /// <inheritdoc />
        public async Task CreateUserAsync(User user)
        {
            ArgumentNullException.ThrowIfNull(user);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
    }
}
