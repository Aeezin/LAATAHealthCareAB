using HealthCareAB_v1.Models.Entities;

namespace HealthCareAB_v1.Repositories.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment> CreateAsync(Appointment appointment);
    Task<List<Appointment>> GetByPatientIdAsync(int patientId);
    Task<List<Appointment>> GetByCaregiverIdAsync(int caregiverId);
    Task<List<Appointment>> GetByCaregiverAndDateRangeAsync(int caregiverId, DateTime startDate, DateTime endDate);
    Task<Appointment?> GetByIdAsync(int id);
    Task UpdateAsync(Appointment appointment);
    Task DeleteAsync(int id);


    Task<int> GetPatientAppointmentCountInLast30DaysAsync(int patientId, DateOnly fromDate);
    Task<bool> HasConflictingAppointmentAsync(
        int caregiverId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime);
}