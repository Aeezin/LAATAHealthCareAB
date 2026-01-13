using HealthCareAB_v1.Models.Entities;
using Microsoft.EntityFrameworkCore;
using HealthCareAB_v1.Models.Enums;
using HealthCareAB_v1.Repositories.Interfaces;

namespace HealthCareAB_v1.Repositories.Implementations;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly AppDbContext _context;

    public AppointmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment> CreateAsync(Appointment appointment)
    {
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        return appointment;
    }

    public async Task<int> GetPatientBookingCountInLast30DaysAsync(int patientId, DateOnly fromDate)
    {
        return await _context.Appointments
            .Where(a => a.PatientId == patientId
                     && a.Date >= fromDate
                     && a.Status != AppointmentStatus.Cancelled) // Do not include cancelled bookings
            .CountAsync();
    }

    public async Task<bool> HasConflictingAppointmentAsync(
        int caregiverId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        return await _context.Appointments
            .Where(a => a.CaregiverId == caregiverId
                     && a.Date == date
                     && a.Status != AppointmentStatus.Cancelled)
            .AnyAsync(a =>
                (startTime >= a.StartTime && startTime < a.EndTime) ||      // New start overlaps existing
                (endTime > a.StartTime && endTime <= a.EndTime) ||          // New end overlaps existing
                (startTime <= a.StartTime && endTime >= a.EndTime));        // New appointment encompasses existing
    }
}