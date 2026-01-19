using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services.Interfaces;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Exceptions;
using HealthCareAB_v1.Models.DTOs;
using HealthCareAB_v1.Models.Enums;

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
        ValidateBasicInput(appointment);
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

        await ValidateEntitiesExistAsync(appointment.PatientId, appointment.CaregiverId);

        await ValidateBusinessRulesAsync(appointment);

        // Schedule & Availability Validation
        await ValidateTimeSlotAvailabilityAsync(
            appointment.CaregiverId,
            appointment.Date,
            appointment.StartTime,
            appointment.EndTime);

        return await _appointmentRepository.CreateAsync(appointment);
    }

    public async Task<List<Appointment>> GetByUserIdAsync(int userId)
    {
        // Try to find Patient for this user
        var patient = await _patientRepository.GetByUserIdAsync(userId);

        if (patient != null)
        {
            return await _appointmentRepository
                .GetByPatientIdAsync(patient.Id);
        }

        // Try to find Caregiver for this user
        var caregiver = await _caregiverRepository.GetByUserIdAsync(userId);

        if (caregiver != null)
        {
            return await _appointmentRepository
                .GetByCaregiverIdAsync(caregiver.Id);
        }

        throw new NotFoundException("User profile not found");
    }

    public async Task<AvailableTimeSlotsResponse> GetAvailableTimeSlotsAsync(int caregiverId, DateTime startDate, DateTime endDate)
    {
        // Get Caregiver
        var caregiver = await _caregiverRepository.GetByIdAsync(caregiverId);
        if (caregiver == null)
        {
            throw new CaregiverNotFoundException($"Caregiver with ID: '{caregiverId} not found.");
        }

        // FR-2.5.1 - Max 90 days in advance
        var maxBookingDate = DateTime.UtcNow.Date.AddDays(90);
        if (endDate > maxBookingDate)
        {
            endDate = maxBookingDate;
        }

        // FR-2.5.2 Minimum 2 hours notice
        var minBookingTime = DateTime.UtcNow.AddHours(2);
        if (startDate < minBookingTime)
        {
            startDate = minBookingTime;
        }

        // Edge case: If after adjustments, range is invalid, return empty
        if (startDate >= endDate)
        {
            return new AvailableTimeSlotsResponse
            {
                CaregiverId = caregiverId,
                CaregiverName = $"{caregiver.FirstName} {caregiver.LastName}",
                AvailableSlots = new List<DailyAvailability>()
            };
        }

        // Get Caregiver's schedule templates
        var schedules = await _scheduleRepository.GetByCaregiverIdAsync(caregiverId);

        if (!schedules.Any())
        {
            return new AvailableTimeSlotsResponse
            {
                CaregiverId = caregiverId,
                CaregiverName = $"{caregiver.FirstName} {caregiver.LastName}",
                AvailableSlots = new List<DailyAvailability>()
            };
        }

        // Expand schedules into concrete time slots
        var allPotentialSlots = ExpandSchedulesToTimeSlots(schedules, startDate, endDate);

        var existingAppointments = await _appointmentRepository
            .GetByCaregiverAndDateRangeAsync(caregiverId, startDate, endDate);

        // Filter out conflicts with existing appointments
        var availableSlots = FilterOutConflicts(allPotentialSlots, existingAppointments);

        // Group by date and map to DTOs
        var groupedByDate = availableSlots
            .GroupBy(slot => slot.Date)
            .Select(group => new DailyAvailability
            {
                Date = group.Key.ToDateTime(TimeOnly.MinValue),
                TimeSlots = group.Select(slot => new TimeSlotDto
                {
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime
                })
                .OrderBy(ts => ts.StartTime)
                .ToList()
            })
            .OrderBy(d => d.Date)
            .ToList();

        return new AvailableTimeSlotsResponse
        {
            CaregiverId = caregiverId,
            CaregiverName = $"{caregiver.FirstName} {caregiver.LastName}",
            AvailableSlots = groupedByDate
        };
    }

    public async Task<Appointment> CompleteAppointmentAsync(int appointmentId, int userId, CompleteAppointmentRequest dto)
    {
        // Resolve Caregiver from UserId
        var caregiver = await _caregiverRepository.GetByUserIdAsync(userId);
        if (caregiver == null)
        {
            throw new UnauthorizedAccessException("User is not a registered caregiver.");
        }

        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);

        if (appointment == null)
        {
            throw new AppointmentNotFoundException($"Appointment with ID {appointmentId} not found.");
        }

        // Validate Caregiver
        if (appointment.CaregiverId != caregiver.Id)
        {
            throw new UnauthorizedAccessException("You are not authorized to complete this appointment.");
        }

        // Validate Status
        if (appointment.Status != AppointmentStatus.Scheduled)
        {
            throw new AppointmentValidationException($"Appointment must be in '{AppointmentStatus.Scheduled}' status to complete. Current status: '{appointment.Status}'.");
        }

        // Validate Time
        var appointmentEndDateTime = appointment.Date.ToDateTime(appointment.EndTime);
        if (DateTime.UtcNow < appointmentEndDateTime)
        {
            throw new AppointmentValidationException("Cannot complete an appointment before its end time.");
        }



        // Update
        appointment.Status = AppointmentStatus.Completed;
        if (!string.IsNullOrEmpty(dto.CaregiverNotes))
        {
            appointment.CaregiverNotes = dto.CaregiverNotes;
        }
        appointment.UpdatedAt = DateTime.UtcNow;

        await _appointmentRepository.UpdateAsync(appointment);

        return appointment;
    }


    // ============================================
    //            HELPER METHODS: CREATE
    //============================================
    private void ValidateBasicInput(Appointment appointment)
    {
        // StartTime < EndTime
        if (appointment.StartTime >= appointment.EndTime)
        {
            throw new AppointmentValidationException("StartTime must be before EndTime.");
        }

        // Must be exactly 30 minutes
        var duration = appointment.EndTime.ToTimeSpan() - appointment.StartTime.ToTimeSpan();
        if (duration != TimeSpan.FromMinutes(30))
        {
            throw new AppointmentValidationException(
                "Appointments must be exactly 30 minutes long.");
        }

        // Must start on 30-minute boundaries (00 or 30)
        if (appointment.StartTime.Minute != 0 && appointment.StartTime.Minute != 30)
        {
            throw new AppointmentValidationException(
                "Appointments must start at :00 or :30 (e.g., 10:00, 10:30).");
        }

        // Date validation
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (appointment.Date < today)
        {
            throw new AppointmentValidationException("Cannot book appointments in the past.");
        }
    }

    private async Task ValidateEntitiesExistAsync(int patientId, int caregiverId)
    {
        // Patient exists
        bool patientExists = await _patientRepository.ExistsAsync(patientId);
        if (!patientExists)
        {
            throw new PatientNotFoundException($"Patient with ID {patientId} not found.");
        }

        //  Caregiver exists
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
            .GetPatientAppointmentCountInLast30DaysAsync(appointment.PatientId, thirtyDaysAgo);

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
        //  Caregiver has a schedule for this day
        var dayOfWeek = date.DayOfWeek;
        var schedule = await _scheduleRepository.GetScheduleForDayAsync(caregiverId, dayOfWeek);

        if (schedule == null)
        {
            throw new AppointmentValidationException(
                $"Caregiver has no schedule available for {dayOfWeek}.");
        }

        // Requested time falls within caregiver's working hours
        if (startTime < schedule.StartTime || endTime > schedule.EndTime)
        {
            throw new AppointmentValidationException(
                $"Requested time slot is outside caregiver's working hours ({schedule.StartTime} - {schedule.EndTime}).");
        }

        //  No conflicting appointments (NFR-2.5.4)
        bool hasConflict = await _appointmentRepository.HasConflictingAppointmentAsync(
            caregiverId, date, startTime, endTime);

        if (hasConflict)
        {
            throw new AppointmentConflictException(
                "The requested time slot is already booked.");
        }
    }

    // ============================================
    //   HELPER METHODS: GET AVAILABLE TIME SLOTS
    //============================================

    // A simple structure which helps hold time slots while processing
    private class TimeSlot
    {
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }

    private List<TimeSlot> ExpandSchedulesToTimeSlots(
        List<CaregiverSchedule> schedules,
        DateTime startDate,
        DateTime endDate)
    {
        var slots = new List<TimeSlot>();

        // Loop through each day in the date range
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var dayOfWeek = date.DayOfWeek;  // Get the day (Monday, Tuesday, etc.)

            // Find schedules for this day of the week
            var daySchedules = schedules
                .Where(s => s.DayOfWeek == dayOfWeek && s.IsActive)
                .ToList();

            foreach (var schedule in daySchedules)
            {
                // Break the schedule into 30-minute slots
                var currentTime = schedule.StartTime;

                while (currentTime < schedule.EndTime)
                {
                    var slotEndTime = currentTime.Add(TimeSpan.FromMinutes(30));

                    // Only add if the slot fits within the schedule
                    if (slotEndTime <= schedule.EndTime)
                    {
                        slots.Add(new TimeSlot
                        {
                            Date = DateOnly.FromDateTime(date),
                            StartTime = currentTime,
                            EndTime = slotEndTime
                        });
                    }

                    currentTime = slotEndTime;
                }
            }
        }

        return slots;
    }

    private bool TimeSlotsOverlap(TimeOnly start1, TimeOnly end1, TimeOnly start2, TimeOnly end2)
    {
        // Two time ranges overlap if:
        // - First slot starts before second slot ends AND
        // - Second slot starts before first slot ends
        return start1 < end2 && start2 < end1;
    }

    private List<TimeSlot> FilterOutConflicts(
        List<TimeSlot> potentialSlots,
        List<Appointment> existingAppointments)
    {
        return potentialSlots.Where(slot =>
        {
            // Check if this slot conflicts with any existing appointment
            var hasConflict = existingAppointments.Any(appt =>
                appt.Date == slot.Date &&
                TimeSlotsOverlap(slot.StartTime, slot.EndTime, appt.StartTime, appt.EndTime)
            );

            // Keep the slot only if there's NO conflict
            return !hasConflict;
        }).ToList();
    }
}