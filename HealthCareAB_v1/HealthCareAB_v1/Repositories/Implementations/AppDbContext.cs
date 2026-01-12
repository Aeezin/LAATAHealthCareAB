using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthCareAB_v1.Repositories.Implementations
{
    public class AppDbContext
        : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>,
            IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Caregiver> Caregivers { get; set; }
        public DbSet<CaregiverSchedule> CaregiverSchedules { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.UseSerialColumns();

            // Patient relationships
            modelBuilder
                .Entity<Patient>()
                .HasOne(p => p.User)
                .WithOne(u => u.Patient)
                .HasForeignKey<Patient>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<Patient>()
                .HasMany(p => p.Appointments)
                .WithOne(a => a.Patient)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Caregiver relationships
            modelBuilder
                .Entity<Caregiver>()
                .HasOne(c => c.User)
                .WithOne(u => u.Caregiver)
                .HasForeignKey<Caregiver>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<Caregiver>()
                .HasMany(c => c.Schedules)
                .WithOne(s => s.Caregiver)
                .HasForeignKey(s => s.CaregiverId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<Caregiver>()
                .HasMany(c => c.Appointments)
                .WithOne(a => a.Caregiver)
                .HasForeignKey(a => a.CaregiverId)
                .OnDelete(DeleteBehavior.Cascade);

            // Appointment - Feedback relationship
            modelBuilder
                .Entity<Appointment>()
                .HasOne(a => a.Feedback)
                .WithOne(f => f.Appointment)
                .HasForeignKey<Feedback>(f => f.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constrants
            modelBuilder.Entity<Patient>().HasIndex(p => p.PersonalIdentityNumber).IsUnique();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
