using HealthCareAB_v1.Models.Entities;

namespace HealthCareAB_v1.Repositories.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment> CreateAsync(Appointment appointment);
}