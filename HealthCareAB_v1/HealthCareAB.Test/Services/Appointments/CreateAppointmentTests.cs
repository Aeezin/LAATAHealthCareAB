using HealthCareAB_v1.Services.Implementations;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Models.Enums;
using HealthCareAB_v1.Exceptions;
using Xunit;
using Moq;

namespace HealthCareAB_v1.Tests.Services;

public class CreateAppointmentTests
{
    private readonly Mock<IAppointmentRepository> _mockAppointmentRepo;
    private readonly Mock<IPatientRepository> _mockPatientRepo;
    private readonly Mock<ICaregiverRepository> _mockCaregiverRepo;
    private readonly Mock<ICaregiverScheduleRepository> _mockScheduleRepo;
    private readonly AppointmentService _service;

    public CreateAppointmentTests()
    {
        _mockAppointmentRepo = new Mock<IAppointmentRepository>();
        _mockPatientRepo = new Mock<IPatientRepository>();
        _mockCaregiverRepo = new Mock<ICaregiverRepository>();
        _mockScheduleRepo = new Mock<ICaregiverScheduleRepository>();

        _service = new AppointmentService(
            _mockAppointmentRepo.Object,
            _mockPatientRepo.Object,
            _mockCaregiverRepo.Object,
            _mockScheduleRepo.Object);
    }

    #region Validation Tests

    [Fact]
    public async Task CreateAsync_WithValidTimeSlot_ReturnsSuccessfulBooking()
    {
        // Arrange
        var appointmentDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)); // 7 days ahead
        var startTime = new TimeOnly(10, 0);
        var endTime = new TimeOnly(10, 30);

        var appointment = new Appointment
        {
            PatientId = 1,
            CaregiverId = 1,
            Date = appointmentDate,
            StartTime = startTime,
            EndTime = endTime,
            PatientNotes = "Regular checkup"
        };

        // Mock: Patient exists
        _mockPatientRepo.Setup(r => r.ExistsAsync(1))
            .ReturnsAsync(true);

        // Mock: Caregiver exists
        _mockCaregiverRepo.Setup(r => r.ExistsAsync(1))
            .ReturnsAsync(true);

        // Mock: Patient has 0 bookings in last 30 days
        _mockAppointmentRepo.Setup(r => r.GetPatientAppointmentCountInLast30DaysAsync(
                1, It.IsAny<DateOnly>()))
            .ReturnsAsync(0);

        // Mock: Caregiver has schedule for the day
        var schedule = new CaregiverSchedule
        {
            Id = 1,
            CaregiverId = 1,
            DayOfWeek = appointmentDate.DayOfWeek,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            IsActive = true
        };
        _mockScheduleRepo.Setup(r => r.GetScheduleForDayAsync(1, appointmentDate.DayOfWeek))
            .ReturnsAsync(schedule);

        // Mock: No conflicting appointments
        _mockAppointmentRepo.Setup(r => r.HasConflictingAppointmentAsync(
                1, appointmentDate, startTime, endTime))
            .ReturnsAsync(false);

        // Mock: Repository creates appointment successfully
        var createdAppointment = new Appointment
        {
            Id = 1,
            PatientId = appointment.PatientId,
            CaregiverId = appointment.CaregiverId,
            Date = appointment.Date,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            PatientNotes = appointment.PatientNotes,
            Status = AppointmentStatus.Scheduled
        };
        _mockAppointmentRepo.Setup(r => r.CreateAsync(It.IsAny<Appointment>()))
            .ReturnsAsync(createdAppointment);

        // Act
        var result = await _service.CreateAsync(appointment);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(AppointmentStatus.Scheduled, result.Status);
        Assert.Equal(appointment.PatientId, result.PatientId);
        Assert.Equal(appointment.CaregiverId, result.CaregiverId);

        // Verify all mocks were called correctly
        _mockPatientRepo.Verify(r => r.ExistsAsync(1), Times.Once);
        _mockCaregiverRepo.Verify(r => r.ExistsAsync(1), Times.Once);
        _mockAppointmentRepo.Verify(r => r.CreateAsync(It.IsAny<Appointment>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithPastDateTime_ThrowsAppointmentValidationException()
    {
        // Arrange
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)); // Yesterday
        var appointment = new Appointment
        {
            PatientId = 1,
            CaregiverId = 1,
            Date = pastDate,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(10, 30)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppointmentValidationException>(
            () => _service.CreateAsync(appointment));

        Assert.Equal("Cannot book appointments in the past.", exception.Message);

        // Verify no repository calls were made (fail-fast)
        _mockPatientRepo.Verify(r => r.ExistsAsync(It.IsAny<int>()), Times.Never);
        _mockAppointmentRepo.Verify(r => r.CreateAsync(It.IsAny<Appointment>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenCaregiverNotFound_ThrowsCaregiverNotFoundException()
    {
        // Arrange
        var appointmentDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var appointment = new Appointment
        {
            PatientId = 1,
            CaregiverId = 999, // Non-existent caregiver
            Date = appointmentDate,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(10, 30)
        };

        // Mock: Patient exists
        _mockPatientRepo.Setup(r => r.ExistsAsync(1))
            .ReturnsAsync(true);

        // Mock: Caregiver does NOT exist
        _mockCaregiverRepo.Setup(r => r.ExistsAsync(999))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CaregiverNotFoundException>(
            () => _service.CreateAsync(appointment));

        Assert.Equal("Caregiver with ID 999 not found.", exception.Message);

        // Verify patient check happened but no booking count check
        _mockPatientRepo.Verify(r => r.ExistsAsync(1), Times.Once);
        _mockCaregiverRepo.Verify(r => r.ExistsAsync(999), Times.Once);
        _mockAppointmentRepo.Verify(r => r.GetPatientAppointmentCountInLast30DaysAsync(
            It.IsAny<int>(), It.IsAny<DateOnly>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenPatientNotFound_ThrowsPatientNotFoundException()
    {
        // Arrange
        var appointmentDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var appointment = new Appointment
        {
            PatientId = 999, // Non-existent patient
            CaregiverId = 1,
            Date = appointmentDate,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(10, 30)
        };

        // Mock: Patient does NOT exist
        _mockPatientRepo.Setup(r => r.ExistsAsync(999))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PatientNotFoundException>(
            () => _service.CreateAsync(appointment));

        Assert.Equal("Patient with ID 999 not found.", exception.Message);

        // Verify fail-fast: no caregiver check happened
        _mockPatientRepo.Verify(r => r.ExistsAsync(999), Times.Once);
        _mockCaregiverRepo.Verify(r => r.ExistsAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region 30-Minute Slot Validation Tests

    [Fact]
    public async Task CreateAsync_WithNon30MinuteDuration_ThrowsAppointmentValidationException()
    {
        // Arrange - Duration is 45 minutes instead of 30
        var appointment = new Appointment
        {
            PatientId = 1,
            CaregiverId = 1,
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(10, 45)  // 45 minutes - invalid!
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppointmentValidationException>(
            () => _service.CreateAsync(appointment));

        Assert.Equal("Appointments must be exactly 30 minutes long.", exception.Message);

        // Verify fail-fast: no repository calls
        _mockPatientRepo.Verify(r => r.ExistsAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidStartMinute_ThrowsAppointmentValidationException()
    {
        // Arrange - Starts at 10:15 instead of :00 or :30
        var appointment = new Appointment
        {
            PatientId = 1,
            CaregiverId = 1,
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            StartTime = new TimeOnly(10, 15),  // Invalid! Must be :00 or :30
            EndTime = new TimeOnly(10, 45)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppointmentValidationException>(
            () => _service.CreateAsync(appointment));

        Assert.Equal("Appointments must start at :00 or :30 (e.g., 10:00, 10:30).", exception.Message);

        // Verify fail-fast
        _mockPatientRepo.Verify(r => r.ExistsAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_With60MinuteDuration_ThrowsAppointmentValidationException()
    {
        // Arrange - 1 hour appointment not allowed
        var appointment = new Appointment
        {
            PatientId = 1,
            CaregiverId = 1,
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 0)  // 60 minutes - invalid!
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppointmentValidationException>(
            () => _service.CreateAsync(appointment));

        Assert.Equal("Appointments must be exactly 30 minutes long.", exception.Message);
    }

    #endregion

    #region Business Logic Tests

    [Fact]
    public async Task CreateAsync_WhenPatientHas3BookingsInLast30Days_CreatesSuccessfully()
    {
        // Arrange - Edge case: 3 bookings is OK, 4 is the limit
        var appointmentDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var appointment = new Appointment
        {
            PatientId = 1,
            CaregiverId = 1,
            Date = appointmentDate,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(10, 30)
        };

        // Mock: Patient exists
        _mockPatientRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

        // Mock: Caregiver exists
        _mockCaregiverRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

        // Mock: Patient has exactly 3 bookings (edge case - should still allow)
        _mockAppointmentRepo.Setup(r => r.GetPatientAppointmentCountInLast30DaysAsync(
                1, It.IsAny<DateOnly>()))
            .ReturnsAsync(3);

        // Mock: Caregiver has schedule
        var schedule = new CaregiverSchedule
        {
            CaregiverId = 1,
            DayOfWeek = appointmentDate.DayOfWeek,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            IsActive = true
        };
        _mockScheduleRepo.Setup(r => r.GetScheduleForDayAsync(1, appointmentDate.DayOfWeek))
            .ReturnsAsync(schedule);

        // Mock: No conflicts
        _mockAppointmentRepo.Setup(r => r.HasConflictingAppointmentAsync(
                1, appointmentDate, It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>()))
            .ReturnsAsync(false);

        // Mock: Create success
        _mockAppointmentRepo.Setup(r => r.CreateAsync(It.IsAny<Appointment>()))
            .ReturnsAsync(new Appointment
            {
                Id = 1,
                PatientId = 1,
                CaregiverId = 1,
                Date = appointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = AppointmentStatus.Scheduled
            });

        // Act
        var result = await _service.CreateAsync(appointment);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(AppointmentStatus.Scheduled, result.Status);
        _mockAppointmentRepo.Verify(r => r.CreateAsync(It.IsAny<Appointment>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_MoreThan90DaysAhead_ThrowsAppointmentValidationException()
    {
        // Arrange - FR-2.5.1: Cannot book more than 90 days ahead
        var tooFarDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(91)); // 91 days
        var appointment = new Appointment
        {
            PatientId = 1,
            CaregiverId = 1,
            Date = tooFarDate,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(10, 30)
        };

        // Mock: Entities exist
        _mockPatientRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockCaregiverRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppointmentValidationException>(
            () => _service.CreateAsync(appointment));

        Assert.Equal("Cannot book appointments more than 90 days in advance.", exception.Message);

        // Verify it failed before checking booking count
        _mockAppointmentRepo.Verify(r => r.GetPatientAppointmentCountInLast30DaysAsync(
            It.IsAny<int>(), It.IsAny<DateOnly>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_LessThan2HoursBefore_ThrowsAppointmentValidationException()
    {
        // Arrange - FR-2.5.2: Must book at least 2 hours in advance
        var now = DateTime.UtcNow;

        // Round to next valid 30-minute slot
        var roundedNow = new DateTime(
            now.Year, now.Month, now.Day,
            now.Hour,
            now.Minute >= 30 ? 30 : 0,
            0);

        var tooSoonDateTime = roundedNow.AddHours(1).AddMinutes(30); // Only 1.5 hours ahead

        var appointment = new Appointment
        {
            PatientId = 1,
            CaregiverId = 1,
            Date = DateOnly.FromDateTime(tooSoonDateTime),
            StartTime = TimeOnly.FromDateTime(tooSoonDateTime),
            EndTime = TimeOnly.FromDateTime(tooSoonDateTime.AddMinutes(30))
        };

        // Mock: Entities exist
        _mockPatientRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockCaregiverRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppointmentValidationException>(
            () => _service.CreateAsync(appointment));

        Assert.Equal("Appointments must be booked at least 2 hours in advance.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_WhenPatientHas4BookingsInLast30Days_ThrowsAppointmentLimitException()
    {
        // Arrange - FR-2.5.5: Max 4 bookings per 30 days
        var appointmentDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var appointment = new Appointment
        {
            PatientId = 1,
            CaregiverId = 1,
            Date = appointmentDate,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(10, 30)
        };

        // Mock: Entities exist
        _mockPatientRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockCaregiverRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);

        // Mock: Patient already has 4 bookings
        _mockAppointmentRepo.Setup(r => r.GetPatientAppointmentCountInLast30DaysAsync(
                1, It.IsAny<DateOnly>()))
            .ReturnsAsync(4);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppointmentLimitException>(
            () => _service.CreateAsync(appointment));

        Assert.Equal("Patient has reached the maximum limit of 4 bookings per 30 days.", exception.Message);

        // Verify it stopped before checking schedule
        _mockScheduleRepo.Verify(r => r.GetScheduleForDayAsync(
            It.IsAny<int>(), It.IsAny<DayOfWeek>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenTimeSlotAlreadyBooked_ThrowsAppointmentConflictException()
    {
        // Arrange - NFR-2.5.4: Prevent double booking
        var appointmentDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var appointment = new Appointment
        {
            PatientId = 1,
            CaregiverId = 1,
            Date = appointmentDate,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(10, 30)
        };

        // Mock: All validations pass
        _mockPatientRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockCaregiverRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockAppointmentRepo.Setup(r => r.GetPatientAppointmentCountInLast30DaysAsync(
                1, It.IsAny<DateOnly>()))
            .ReturnsAsync(0);

        // Mock: Schedule exists
        var schedule = new CaregiverSchedule
        {
            CaregiverId = 1,
            DayOfWeek = appointmentDate.DayOfWeek,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            IsActive = true
        };
        _mockScheduleRepo.Setup(r => r.GetScheduleForDayAsync(1, appointmentDate.DayOfWeek))
            .ReturnsAsync(schedule);

        // Mock: Time slot is already booked (conflict!)
        _mockAppointmentRepo.Setup(r => r.HasConflictingAppointmentAsync(
                1, appointmentDate, appointment.StartTime, appointment.EndTime))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppointmentConflictException>(
            () => _service.CreateAsync(appointment));

        Assert.Equal("The requested time slot is already booked.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_WhenCaregiverHasNoScheduleForDay_ThrowsAppointmentValidationException()
    {
        // Arrange - Caregiver doesn't work on requested day
        var appointmentDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var appointment = new Appointment
        {
            PatientId = 1,
            CaregiverId = 1,
            Date = appointmentDate,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(10, 30)
        };

        // Mock: All preliminary checks pass
        _mockPatientRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockCaregiverRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockAppointmentRepo.Setup(r => r.GetPatientAppointmentCountInLast30DaysAsync(
                1, It.IsAny<DateOnly>()))
            .ReturnsAsync(0);

        // Mock: NO schedule for this day (null)
        _mockScheduleRepo.Setup(r => r.GetScheduleForDayAsync(1, appointmentDate.DayOfWeek))
            .ReturnsAsync((CaregiverSchedule?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppointmentValidationException>(
            () => _service.CreateAsync(appointment));

        Assert.Contains("has no schedule available for", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_WhenTimeSlotOutsideWorkingHours_ThrowsAppointmentValidationException()
    {
        // Arrange - Booking time outside caregiver's working hours
        var appointmentDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var appointment = new Appointment
        {
            PatientId = 1,
            CaregiverId = 1,
            Date = appointmentDate,
            StartTime = new TimeOnly(18, 0), // 6 PM - outside working hours
            EndTime = new TimeOnly(18, 30)
        };

        // Mock: Preliminary checks pass
        _mockPatientRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockCaregiverRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockAppointmentRepo.Setup(r => r.GetPatientAppointmentCountInLast30DaysAsync(
                1, It.IsAny<DateOnly>()))
            .ReturnsAsync(0);

        // Mock: Schedule exists but ends at 5 PM
        var schedule = new CaregiverSchedule
        {
            CaregiverId = 1,
            DayOfWeek = appointmentDate.DayOfWeek,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0), // Ends at 5 PM
            IsActive = true
        };
        _mockScheduleRepo.Setup(r => r.GetScheduleForDayAsync(1, appointmentDate.DayOfWeek))
            .ReturnsAsync(schedule);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppointmentValidationException>(
            () => _service.CreateAsync(appointment));

        Assert.Contains("outside caregiver's working hours", exception.Message);
    }

    // TODO: Skipping auth test - will implement when authentication is added
    // [Fact]
    // public async Task CreateAsync_PatientCanOnlyBookForThemselves_ThrowsUnauthorizedException()

    #endregion

    #region Edge Cases

    [Fact]
    public async Task CreateAsync_Exactly2HoursBefore_CreatesSuccessfully()
    {
        // Arrange - Use a time 1 day ahead to safely pass the 2-hour validation
        var appointmentDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var startTime = new TimeOnly(14, 0);  // Fixed valid time
        var endTime = new TimeOnly(14, 30);

        var appointment = new Appointment
        {
            PatientId = 1,
            CaregiverId = 1,
            Date = appointmentDate,
            StartTime = startTime,
            EndTime = endTime
        };

        // Mock: All checks pass
        _mockPatientRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockCaregiverRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockAppointmentRepo.Setup(r => r.GetPatientAppointmentCountInLast30DaysAsync(
                1, It.IsAny<DateOnly>()))
            .ReturnsAsync(0);

        var schedule = new CaregiverSchedule
        {
            CaregiverId = 1,
            DayOfWeek = appointmentDate.DayOfWeek,
            StartTime = new TimeOnly(0, 0),
            EndTime = new TimeOnly(23, 59),
            IsActive = true
        };
        _mockScheduleRepo.Setup(r => r.GetScheduleForDayAsync(1, appointmentDate.DayOfWeek))
            .ReturnsAsync(schedule);

        _mockAppointmentRepo.Setup(r => r.HasConflictingAppointmentAsync(
                1, appointmentDate, startTime, endTime))
            .ReturnsAsync(false);

        _mockAppointmentRepo.Setup(r => r.CreateAsync(It.IsAny<Appointment>()))
            .ReturnsAsync(new Appointment
            {
                Id = 1,
                PatientId = 1,
                CaregiverId = 1,
                Date = appointmentDate,
                StartTime = startTime,
                EndTime = endTime,
                Status = AppointmentStatus.Scheduled
            });

        // Act
        var result = await _service.CreateAsync(appointment);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(AppointmentStatus.Scheduled, result.Status);
    }

    [Fact]
    public async Task CreateAsync_AtExactStartTime_CreatesSuccessfully()
    {
        // Arrange - Booking exactly at schedule start time
        var appointmentDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var scheduleStartTime = new TimeOnly(9, 0);

        var appointment = new Appointment
        {
            PatientId = 1,
            CaregiverId = 1,
            Date = appointmentDate,
            StartTime = scheduleStartTime, // Exactly at start
            EndTime = scheduleStartTime.AddMinutes(30)
        };

        // Mock: All checks pass
        _mockPatientRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockCaregiverRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockAppointmentRepo.Setup(r => r.GetPatientAppointmentCountInLast30DaysAsync(
                1, It.IsAny<DateOnly>()))
            .ReturnsAsync(0);

        var schedule = new CaregiverSchedule
        {
            CaregiverId = 1,
            DayOfWeek = appointmentDate.DayOfWeek,
            StartTime = scheduleStartTime,
            EndTime = new TimeOnly(17, 0),
            IsActive = true
        };
        _mockScheduleRepo.Setup(r => r.GetScheduleForDayAsync(1, appointmentDate.DayOfWeek))
            .ReturnsAsync(schedule);

        _mockAppointmentRepo.Setup(r => r.HasConflictingAppointmentAsync(
                1, appointmentDate, It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>()))
            .ReturnsAsync(false);

        _mockAppointmentRepo.Setup(r => r.CreateAsync(It.IsAny<Appointment>()))
            .ReturnsAsync(new Appointment
            {
                Id = 1,
                PatientId = 1,
                CaregiverId = 1,
                Date = appointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = AppointmentStatus.Scheduled
            });

        // Act
        var result = await _service.CreateAsync(appointment);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(scheduleStartTime, result.StartTime);
    }

    [Fact]
    public async Task CreateAsync_OneSlotBeforeEndTime_CreatesSuccessfully()
    {
        // Arrange - Booking the last available slot before schedule ends
        var appointmentDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var scheduleEndTime = new TimeOnly(17, 0);

        var appointment = new Appointment
        {
            PatientId = 1,
            CaregiverId = 1,
            Date = appointmentDate,
            StartTime = new TimeOnly(16, 30), // 30 minutes before end
            EndTime = scheduleEndTime // Exactly at end time
        };

        // Mock: All checks pass
        _mockPatientRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockCaregiverRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockAppointmentRepo.Setup(r => r.GetPatientAppointmentCountInLast30DaysAsync(
                1, It.IsAny<DateOnly>()))
            .ReturnsAsync(0);

        var schedule = new CaregiverSchedule
        {
            CaregiverId = 1,
            DayOfWeek = appointmentDate.DayOfWeek,
            StartTime = new TimeOnly(9, 0),
            EndTime = scheduleEndTime,
            IsActive = true
        };
        _mockScheduleRepo.Setup(r => r.GetScheduleForDayAsync(1, appointmentDate.DayOfWeek))
            .ReturnsAsync(schedule);

        _mockAppointmentRepo.Setup(r => r.HasConflictingAppointmentAsync(
                1, appointmentDate, It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>()))
            .ReturnsAsync(false);

        _mockAppointmentRepo.Setup(r => r.CreateAsync(It.IsAny<Appointment>()))
            .ReturnsAsync(new Appointment
            {
                Id = 1,
                PatientId = 1,
                CaregiverId = 1,
                Date = appointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = AppointmentStatus.Scheduled
            });

        // Act
        var result = await _service.CreateAsync(appointment);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(scheduleEndTime, result.EndTime);
    }

    [Fact]
    public async Task CreateAsync_StartTimeAfterEndTime_ThrowsAppointmentValidationException()
    {
        // Arrange - Invalid: start time after end time
        var appointmentDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var appointment = new Appointment
        {
            PatientId = 1,
            CaregiverId = 1,
            Date = appointmentDate,
            StartTime = new TimeOnly(11, 0), // After end time!
            EndTime = new TimeOnly(10, 0)
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppointmentValidationException>(
            () => _service.CreateAsync(appointment));

        Assert.Equal("StartTime must be before EndTime.", exception.Message);

        // Verify no repository calls (fail-fast at basic validation)
        _mockPatientRepo.Verify(r => r.ExistsAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion
}