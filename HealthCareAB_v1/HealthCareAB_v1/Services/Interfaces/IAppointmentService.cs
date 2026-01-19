using HealthCareAB_v1.Models.DTOs;
using HealthCareAB_v1.Models.Entities;

namespace HealthCareAB_v1.Services.Interfaces;

public interface IAppointmentService
{
    Task<Appointment> CreateAsync(Appointment appointment);
    Task<List<Appointment>> GetByUserIdAsync(int userId);
    Task<AvailableTimeSlotsResponse> GetAvailableTimeSlotsAsync(int caregiverId, DateTime startDate, DateTime endDate);

}