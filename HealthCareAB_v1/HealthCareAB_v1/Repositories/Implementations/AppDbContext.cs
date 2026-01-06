using System;
using System.Text.Json;
using HealthCareAB_v1.Models;
using HealthCareAB_v1.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HealthCareAB_v1.Repositories.Implementations
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseSerialColumns();

            var rolesConverter = new ValueConverter<List<string>, string>(
                v =>
                    JsonSerializer.Serialize(
                        v,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    ),
                v =>
                    JsonSerializer.Deserialize<List<string>>(
                        v,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    ) ?? new List<string>()
            );

            modelBuilder
                .Entity<User>()
                .Property(e => e.Roles)
                .HasConversion(rolesConverter)
                .HasColumnType("jsonb");
        }

        public DbSet<User> Users { get; set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
