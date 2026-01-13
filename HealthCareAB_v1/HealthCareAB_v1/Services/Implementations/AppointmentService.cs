using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Repositories.Interfaces;

namespace HealthCareAB_v1.Services;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;

    public AppointmentService(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<Appointment> CreateAsync(Appointment appointment)
    {
        // : Basic Input Validation
        // - StartTime < EndTime
        // - Date not in the past

        // : Entity Existence Checks
        // - Patient exists
        // - Caregiver exists

        // : Business Rule Validations
        // - Not more than 90 days ahead (FR-2.5.1)
        // - At least 2 hours in advance (FR-2.5.2)
        // - Patient hasn't exceeded 4 bookings/month (FR-2.5.5)

        // : Schedule & Availability Validation
        // - Caregiver has a schedule for that day
        // - Requested time slot falls within working hours
        // - No conflicting appointments (NFR-2.5.4)
    }