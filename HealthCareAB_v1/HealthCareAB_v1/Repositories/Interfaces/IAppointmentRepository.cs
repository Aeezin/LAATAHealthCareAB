using HealthCareAB_v1.Models.Entities;

namespace HealthCareAB_v1.Repositories.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment> CreateAsync(Appointment appointment);

    Task<int> GetPatientBookingCountInLast30DaysAsync(int patientId, DateOnly fromDate);
    Task<bool> HasConflictingAppointmentAsync(
        int caregiverId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime);
}