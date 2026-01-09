using System;
using HealthCareAB_v1.Models;
using HealthCareAB_v1.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthCareAB_v1.Repositories.Interfaces
{
    public interface IAppDbContext
    {
        DbSet<ApplicationUser> Users { get; set; }
        DbSet<Patient> Patients { get; set; }
        DbSet<Caregiver> Caregivers { get; set; }
        DbSet<CaregiverSchedule> CaregiverSchedules { get; set; }
        DbSet<Appointment> Appointments { get; set; }
        DbSet<Feedback> Feedbacks { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
