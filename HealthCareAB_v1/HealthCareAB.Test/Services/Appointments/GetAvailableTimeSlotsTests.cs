using HealthCareAB_v1.Services.Implementations;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Models.Enums;
using HealthCareAB_v1.Exceptions;
using Moq;
using Xunit;

namespace HealthCareAB_v1.Tests.ServiceTests;

public class GetAvailableTimeSlotsTests
{
    private readonly Mock<IAppointmentRepository> _mockAppointmentRepo;
    private readonly Mock<IPatientRepository> _mockPatientRepo;
    private readonly Mock<ICaregiverRepository> _mockCaregiverRepo;
    private readonly Mock<ICaregiverScheduleRepository> _mockScheduleRepo;
    private readonly AppointmentService _service;

    public GetAvailableTimeSlotsTests()
    {
        _mockAppointmentRepo = new Mock<IAppointmentRepository>();
        _mockPatientRepo = new Mock<IPatientRepository>();
        _mockCaregiverRepo = new Mock<ICaregiverRepository>();
        _mockScheduleRepo = new Mock<ICaregiverScheduleRepository>();

        _service = new AppointmentService(
            _mockAppointmentRepo.Object,
            _mockPatientRepo.Object,
            _mockCaregiverRepo.Object,
            _mockScheduleRepo.Object
        );
    }

    // ═══════════════════════════════════════════════════════════════
    // POSITIVE CASES
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetAvailableTimeSlotsAsync_ValidRequest_ReturnsAvailableSlots()
    {
        // Arrange
        var caregiverId = 1;
        var startDate = DateTime.UtcNow.AddDays(3);
        var endDate = DateTime.UtcNow.AddDays(4);

        var caregiver = new Caregiver
        {
            Id = caregiverId,
            UserId = 1,
            FirstName = "Anna",
            LastName = "Andersson",
            Specialisation = "General",
            Room = "101"
        };

        var schedules = new List<CaregiverSchedule>
        {
            new CaregiverSchedule
            {
                Id = 1,
                CaregiverId = caregiverId,
                DayOfWeek = startDate.DayOfWeek,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0),
                IsActive = true
            }
        };

        _mockCaregiverRepo
            .Setup(r => r.GetByIdAsync(caregiverId))
            .ReturnsAsync(caregiver);

        _mockScheduleRepo
            .Setup(r => r.GetByCaregiverIdAsync(caregiverId))
            .ReturnsAsync(schedules);

        _mockAppointmentRepo
            .Setup(r => r.GetByCaregiverAndDateRangeAsync(caregiverId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Appointment>());

        // Act
        var result = await _service.GetAvailableTimeSlotsAsync(caregiverId, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(caregiverId, result.CaregiverId);
        Assert.Equal("Anna Andersson", result.CaregiverName);
        Assert.NotEmpty(result.AvailableSlots);
        Assert.All(result.AvailableSlots, day => Assert.NotEmpty(day.TimeSlots));
    }

    [Fact]
    public async Task GetAvailableTimeSlotsAsync_WithExistingAppointments_FiltersOutConflicts()
    {
        // Arrange
        var caregiverId = 1;
        var startDate = DateTime.UtcNow.AddDays(3);
        var endDate = DateTime.UtcNow.AddDays(3);
        var appointmentDate = DateOnly.FromDateTime(startDate);

        var caregiver = new Caregiver
        {
            Id = caregiverId,
            UserId = 1,
            FirstName = "Anna",
            LastName = "Andersson",
            Specialisation = "General",
            Room = "101"
        };

        var schedules = new List<CaregiverSchedule>
        {
            new CaregiverSchedule
            {
                Id = 1,
                CaregiverId = caregiverId,
                DayOfWeek = startDate.DayOfWeek,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                IsActive = true
            }
        };

        // One booked appointment 09:00-09:30
        var existingAppointments = new List<Appointment>
        {
            new Appointment
            {
                Id = 1,
                PatientId = 1,
                CaregiverId = caregiverId,
                Date = appointmentDate,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(9, 30),
                Status = AppointmentStatus.Scheduled
            }
        };

        _mockCaregiverRepo
            .Setup(r => r.GetByIdAsync(caregiverId))
            .ReturnsAsync(caregiver);

        _mockScheduleRepo
            .Setup(r => r.GetByCaregiverIdAsync(caregiverId))
            .ReturnsAsync(schedules);

        _mockAppointmentRepo
            .Setup(r => r.GetByCaregiverAndDateRangeAsync(caregiverId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(existingAppointments);

        // Act
        var result = await _service.GetAvailableTimeSlotsAsync(caregiverId, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.AvailableSlots); // One day
        var daySlots = result.AvailableSlots.First().TimeSlots;
        Assert.Single(daySlots); // Only 09:30-10:00 available (09:00-09:30 is booked)
        Assert.Equal(new TimeOnly(9, 30), daySlots.First().StartTime);
    }

    // ═══════════════════════════════════════════════════════════════
    // NEGATIVE CASES
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetAvailableTimeSlotsAsync_CaregiverNotFound_ThrowsCaregiverNotFoundException()
    {
        // Arrange
        var caregiverId = 999;
        var startDate = DateTime.UtcNow.AddDays(3);
        var endDate = DateTime.UtcNow.AddDays(4);

        _mockCaregiverRepo
            .Setup(r => r.GetByIdAsync(caregiverId))
            .ReturnsAsync((Caregiver)null!);

        // Act & Assert
        await Assert.ThrowsAsync<CaregiverNotFoundException>(
            () => _service.GetAvailableTimeSlotsAsync(caregiverId, startDate, endDate)
        );
    }

    // ═══════════════════════════════════════════════════════════════
    // EDGE CASES
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetAvailableTimeSlotsAsync_CancelledAppointmentsByCaregiver_BlocksSlots()
    {
        // Arrange
        var caregiverId = 1;
        var startDate = DateTime.UtcNow.AddDays(3);
        var endDate = DateTime.UtcNow.AddDays(3);
        var appointmentDate = DateOnly.FromDateTime(startDate);

        var caregiver = new Caregiver
        {
            Id = caregiverId,
            UserId = 1,
            FirstName = "Anna",
            LastName = "Andersson",
            Specialisation = "General",
            Room = "101"
        };

        var schedules = new List<CaregiverSchedule>
    {
        new CaregiverSchedule
        {
            Id = 1,
            CaregiverId = caregiverId,
            DayOfWeek = startDate.DayOfWeek,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            IsActive = true
        }
    };

        // Cancelled appointment should STILL block the slot (caregiver cancelled)
        var existingAppointments = new List<Appointment>
    {
        new Appointment
        {
            Id = 1,
            PatientId = 1,
            CaregiverId = caregiverId,
            Date = appointmentDate,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(9, 30),
            Status = AppointmentStatus.Cancelled  // Caregiver cancelled - slot stays blocked
        }
    };

        _mockCaregiverRepo
            .Setup(r => r.GetByIdAsync(caregiverId))
            .ReturnsAsync(caregiver);

        _mockScheduleRepo
            .Setup(r => r.GetByCaregiverIdAsync(caregiverId))
            .ReturnsAsync(schedules);

        _mockAppointmentRepo
            .Setup(r => r.GetByCaregiverAndDateRangeAsync(caregiverId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(existingAppointments);

        // Act
        var result = await _service.GetAvailableTimeSlotsAsync(caregiverId, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.AvailableSlots); // One day
        var daySlots = result.AvailableSlots.First().TimeSlots;
        Assert.Single(daySlots); // Only 09:30-10:00 available (09:00-09:30 blocked by cancelled appointment)
        Assert.Equal(new TimeOnly(9, 30), daySlots.First().StartTime);
    }

    [Fact]
    public async Task GetAvailableTimeSlotsAsync_NoSchedules_ReturnsEmptyAvailableSlots()
    {
        // Arrange
        var caregiverId = 1;
        var startDate = DateTime.UtcNow.AddDays(3);
        var endDate = DateTime.UtcNow.AddDays(4);

        var caregiver = new Caregiver
        {
            Id = caregiverId,
            UserId = 1,
            FirstName = "Anna",
            LastName = "Andersson",
            Specialisation = "General",
            Room = "101"
        };

        _mockCaregiverRepo
            .Setup(r => r.GetByIdAsync(caregiverId))
            .ReturnsAsync(caregiver);

        _mockScheduleRepo
            .Setup(r => r.GetByCaregiverIdAsync(caregiverId))
            .ReturnsAsync(new List<CaregiverSchedule>()); // Empty schedules

        // Act
        var result = await _service.GetAvailableTimeSlotsAsync(caregiverId, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(caregiverId, result.CaregiverId);
        Assert.Empty(result.AvailableSlots);
    }

    [Fact]
    public async Task GetAvailableTimeSlotsAsync_InactiveSchedule_ExcludesFromSlots()
    {
        // Arrange
        var caregiverId = 1;
        var startDate = DateTime.UtcNow.AddDays(3);
        var endDate = DateTime.UtcNow.AddDays(3);

        var caregiver = new Caregiver
        {
            Id = caregiverId,
            UserId = 1,
            FirstName = "Anna",
            LastName = "Andersson",
            Specialisation = "General",
            Room = "101"
        };

        var schedules = new List<CaregiverSchedule>
        {
            new CaregiverSchedule
            {
                Id = 1,
                CaregiverId = caregiverId,
                DayOfWeek = startDate.DayOfWeek,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0),
                IsActive = false // Inactive!
            }
        };

        _mockCaregiverRepo
            .Setup(r => r.GetByIdAsync(caregiverId))
            .ReturnsAsync(caregiver);

        _mockScheduleRepo
            .Setup(r => r.GetByCaregiverIdAsync(caregiverId))
            .ReturnsAsync(schedules);

        _mockAppointmentRepo
            .Setup(r => r.GetByCaregiverAndDateRangeAsync(caregiverId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Appointment>());

        // Act
        var result = await _service.GetAvailableTimeSlotsAsync(caregiverId, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.AvailableSlots); // No active schedules = no slots
    }

    [Fact]
    public async Task GetAvailableTimeSlotsAsync_AllSlotsBooked_ReturnsEmptySlots()
    {
        // Arrange
        var caregiverId = 1;
        var startDate = DateTime.UtcNow.AddDays(3);
        var endDate = DateTime.UtcNow.AddDays(3);
        var appointmentDate = DateOnly.FromDateTime(startDate);

        var caregiver = new Caregiver
        {
            Id = caregiverId,
            UserId = 1,
            FirstName = "Anna",
            LastName = "Andersson",
            Specialisation = "General",
            Room = "101"
        };

        var schedules = new List<CaregiverSchedule>
        {
            new CaregiverSchedule
            {
                Id = 1,
                CaregiverId = caregiverId,
                DayOfWeek = startDate.DayOfWeek,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                IsActive = true
            }
        };

        // Both slots booked
        var existingAppointments = new List<Appointment>
        {
            new Appointment
            {
                Id = 1,
                PatientId = 1,
                CaregiverId = caregiverId,
                Date = appointmentDate,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(9, 30),
                Status = AppointmentStatus.Scheduled
            },
            new Appointment
            {
                Id = 2,
                PatientId = 2,
                CaregiverId = caregiverId,
                Date = appointmentDate,
                StartTime = new TimeOnly(9, 30),
                EndTime = new TimeOnly(10, 0),
                Status = AppointmentStatus.Scheduled
            }
        };

        _mockCaregiverRepo
            .Setup(r => r.GetByIdAsync(caregiverId))
            .ReturnsAsync(caregiver);

        _mockScheduleRepo
            .Setup(r => r.GetByCaregiverIdAsync(caregiverId))
            .ReturnsAsync(schedules);

        _mockAppointmentRepo
            .Setup(r => r.GetByCaregiverAndDateRangeAsync(caregiverId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(existingAppointments);

        // Act
        var result = await _service.GetAvailableTimeSlotsAsync(caregiverId, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.AvailableSlots); // All slots booked
    }

    // ═══════════════════════════════════════════════════════════════
    // BUSINESS RULES
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task GetAvailableTimeSlotsAsync_EndDateBeyond90Days_CapsToMaxBookingDate()
    {
        // Arrange
        var caregiverId = 1;
        var startDate = DateTime.UtcNow.AddDays(3);
        var endDate = DateTime.UtcNow.AddDays(120); // Beyond 90 days

        var caregiver = new Caregiver
        {
            Id = caregiverId,
            UserId = 1,
            FirstName = "Anna",
            LastName = "Andersson",
            Specialisation = "General",
            Room = "101"
        };

        var schedules = new List<CaregiverSchedule>
        {
            new CaregiverSchedule
            {
                Id = 1,
                CaregiverId = caregiverId,
                DayOfWeek = startDate.DayOfWeek,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0),
                IsActive = true
            }
        };

        _mockCaregiverRepo
            .Setup(r => r.GetByIdAsync(caregiverId))
            .ReturnsAsync(caregiver);

        _mockScheduleRepo
            .Setup(r => r.GetByCaregiverIdAsync(caregiverId))
            .ReturnsAsync(schedules);

        _mockAppointmentRepo
            .Setup(r => r.GetByCaregiverAndDateRangeAsync(caregiverId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Appointment>());

        // Act
        var result = await _service.GetAvailableTimeSlotsAsync(caregiverId, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        // Should have slots, but only up to 90 days (verify last date is not beyond 90 days)
        var lastDate = result.AvailableSlots.Last().Date;
        var maxAllowedDate = DateTime.UtcNow.Date.AddDays(90);
        Assert.True(lastDate <= maxAllowedDate);
    }

    [Fact]
    public async Task GetAvailableTimeSlotsAsync_StartDateWithin2Hours_AdjustsToMinBookingTime()
    {
        // Arrange
        var caregiverId = 1;
        var startDate = DateTime.UtcNow.AddMinutes(30); // Within 2 hours
        var endDate = DateTime.UtcNow.AddDays(1);

        var caregiver = new Caregiver
        {
            Id = caregiverId,
            UserId = 1,
            FirstName = "Anna",
            LastName = "Andersson",
            Specialisation = "General",
            Room = "101"
        };

        var today = DateTime.UtcNow.AddHours(2).DayOfWeek;
        var tomorrow = DateTime.UtcNow.AddDays(1).DayOfWeek;

        var schedules = new List<CaregiverSchedule>
{
    new CaregiverSchedule
    {
        Id = 1,
        CaregiverId = caregiverId,
        DayOfWeek = today,
        StartTime = new TimeOnly(9, 0),
        EndTime = new TimeOnly(17, 0),
        IsActive = true
    },
    new CaregiverSchedule
    {
        Id = 2,
        CaregiverId = caregiverId,
        DayOfWeek = tomorrow,
        StartTime = new TimeOnly(9, 0),
        EndTime = new TimeOnly(17, 0),
        IsActive = true
    }
};

        _mockCaregiverRepo
            .Setup(r => r.GetByIdAsync(caregiverId))
            .ReturnsAsync(caregiver);

        _mockScheduleRepo
            .Setup(r => r.GetByCaregiverIdAsync(caregiverId))
            .ReturnsAsync(schedules);

        _mockAppointmentRepo
            .Setup(r => r.GetByCaregiverAndDateRangeAsync(caregiverId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new List<Appointment>());

        // Act
        var result = await _service.GetAvailableTimeSlotsAsync(caregiverId, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        // Service should return available slots (business rule adjusts dates, so slots should exist)
        Assert.NotEmpty(result.AvailableSlots);
    }

    [Fact]
    public async Task GetAvailableTimeSlotsAsync_StartDateAfterAdjustedEndDate_ReturnsEmpty()
    {
        // Arrange
        var caregiverId = 1;
        var startDate = DateTime.UtcNow.AddDays(91); // Beyond max
        var endDate = DateTime.UtcNow.AddDays(92);

        var caregiver = new Caregiver
        {
            Id = caregiverId,
            UserId = 1,
            FirstName = "Anna",
            LastName = "Andersson",
            Specialisation = "General",
            Room = "101"
        };

        _mockCaregiverRepo
            .Setup(r => r.GetByIdAsync(caregiverId))
            .ReturnsAsync(caregiver);

        // Act
        var result = await _service.GetAvailableTimeSlotsAsync(caregiverId, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.AvailableSlots); // After adjustments, startDate >= endDate
    }
}