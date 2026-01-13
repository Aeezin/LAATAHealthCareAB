using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services.Interfaces;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Exceptions;

namespace HealthCareAB_v1.Services.Implementations;

public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly ICaregiverRepository _caregiverRepository;
    private readonly ICaregiverScheduleRepository _scheduleRepository;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IPatientRepository patientRepository,
        ICaregiverRepository caregiverRepository,
        ICaregiverScheduleRepository scheduleRepository)
    {
        _appointmentRepository = appointmentRepository;
        _patientRepository = patientRepository;
        _caregiverRepository = caregiverRepository;
        _scheduleRepository = scheduleRepository;
    }

    public async Task<Appointment> CreateAsync(Appointment appointment)
    {
        // PHASE 1: Basic Input Validation
        // Validation: StartTime < EndTime
        if (appointment.StartTime >= appointment.EndTime)
        {
            throw new AppointmentValidationException("StartTime must be before EndTime.");
        }

        // Validation: Date cannot be in the past
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (appointment.Date < today)
        {
            throw new AppointmentValidationException("Cannot book appointments in the past.");
        }

        //  Entity Existence Checks
        await ValidateEntitiesExistAsync(appointment.PatientId, appointment.CaregiverId);

        // Business Rule Validations
        await ValidateBusinessRulesAsync(appointment);

        // Schedule & Availability Validation
        await ValidateTimeSlotAvailabilityAsync(
            appointment.CaregiverId,
            appointment.Date,
            appointment.StartTime,
            appointment.EndTime);

        // PHASE 5: Create Appointment
        return await _appointmentRepository.CreateAsync(appointment);
    }

    private async Task ValidateEntitiesExistAsync(int patientId, int caregiverId)
    {
        // Validation: Patient exists
        bool patientExists = await _patientRepository.ExistsAsync(patientId);
        if (!patientExists)
        {
            throw new PatientNotFoundException($"Patient with ID {patientId} not found.");
        }

        // Validation: Caregiver exists
        bool caregiverExists = await _caregiverRepository.ExistsAsync(caregiverId);
        if (!caregiverExists)
        {
            throw new CaregiverNotFoundException($"Caregiver with ID {caregiverId} not found.");
        }
    }

    private async Task ValidateBusinessRulesAsync(Appointment appointment)
    {
        var now = DateTime.UtcNow;
        var appointmentDateTime = appointment.Date.ToDateTime(appointment.StartTime);

        // FR-2.5.1: Patients can book up to 90 days in advance
        var maxBookingDate = DateOnly.FromDateTime(now.AddDays(90));
        if (appointment.Date > maxBookingDate)
        {
            throw new AppointmentValidationException(
                "Cannot book appointments more than 90 days in advance.");
        }

        // FR-2.5.2: Patients can book at least 2 hours in advance
        var minBookingTime = now.AddHours(2);
        if (appointmentDateTime < minBookingTime)
        {
            throw new AppointmentValidationException(
                "Appointments must be booked at least 2 hours in advance.");
        }

        // FR-2.5.5: Patients can book max 4 times per 30 days
        var thirtyDaysAgo = DateOnly.FromDateTime(now.AddDays(-30));
        int bookingCount = await _appointmentRepository
            .GetPatientBookingCountInLast30DaysAsync(appointment.PatientId, thirtyDaysAgo);

        if (bookingCount >= 4)
        {
            throw new AppointmentLimitException(
                "Patient has reached the maximum limit of 4 bookings per 30 days.");
        }

        // TODO: NFR-2.5.1 - Validate patient can only book for themselves
        // This will be implemented when authentication is added
        // Something like...
        // if (appointment.PatientId != authenticatedPatientId)
        //     throw new UnauthorizedException("You can only book appointments for yourself");
    }

    private async Task ValidateTimeSlotAvailabilityAsync(
        int caregiverId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        // Validation 1: Caregiver has a schedule for this day
        var dayOfWeek = date.DayOfWeek;
        var schedule = await _scheduleRepository.GetScheduleForDayAsync(caregiverId, dayOfWeek);

        if (schedule == null)
        {
            throw new AppointmentValidationException(
                $"Caregiver has no schedule available for {dayOfWeek}.");
        }

        // Validation 2: Requested time falls within caregiver's working hours
        if (startTime < schedule.StartTime || endTime > schedule.EndTime)
        {
            throw new AppointmentValidationException(
                $"Requested time slot is outside caregiver's working hours ({schedule.StartTime} - {schedule.EndTime}).");
        }

        // Validation 3: No conflicting appointments (NFR-2.5.4)
        bool hasConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
            caregiverId, date, startTime, endTime);

        if (hasConflict)
        {
            throw new AppointmentConflictException(
                "The requested time slot is already booked.");
        }
    }
}