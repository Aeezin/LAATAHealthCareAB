using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Repositories.Implementations;
using HealthCareAB_v1.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HealthCareAB.Test.Helpers
{
    
public sealed class TestDbContextAdapter : IAppDbContext
    {
        private readonly AppDbContext _inner;
        private readonly IDbContextTransaction _tx;

        public TestDbContextAdapter(AppDbContext inner, IDbContextTransaction tx)
        {
            _inner = inner;
            _tx = tx;
        }

        public DbSet<ApplicationUser> Users
        {
            get => _inner.Users;
            set => _inner.Users = value;
        }

        public DbSet<Patient> Patients
        {
            get => _inner.Patients;
            set => _inner.Patients = value;
        }

        public DbSet<Caregiver> Caregivers
        {
            get => _inner.Caregivers;
            set => _inner.Caregivers = value;
        }

        public DbSet<CaregiverSchedule> CaregiverSchedules
        {
            get => _inner.CaregiverSchedules;
            set => _inner.CaregiverSchedules = value;
        }

        public DbSet<Appointment> Appointments
        {
            get => _inner.Appointments;
            set => _inner.Appointments = value;
        }

        public DbSet<Feedback> Feedbacks
        {
            get => _inner.Feedbacks;
            set => _inner.Feedbacks = value;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => _inner.SaveChangesAsync(cancellationToken);

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_tx);
    }
}