using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Models.DTOs.Appointment;

namespace HealthCareAB_v1.Services.Interfaces;

public interface IAppointmentService
{
    Task<Appointment> CreateAsync(Appointment appointment);
    Task<List<Appointment>> GetByUserIdAsync(int userId);
    Task<Appointment> CompleteAppointmentAsync(int appointmentId, int userId, CompleteAppointmentRequest dto);
}