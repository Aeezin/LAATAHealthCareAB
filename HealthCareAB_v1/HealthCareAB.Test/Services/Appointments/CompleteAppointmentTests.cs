using HealthCareAB_v1.Services.Implementations;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Models.Enums;
using HealthCareAB_v1.Exceptions;
using HealthCareAB_v1.Models.DTOs;
using Xunit;
using Moq;

namespace HealthCareAB_v1.Tests.Services;

public class CompleteAppointmentTests
{
    private readonly Mock<IAppointmentRepository> _mockAppointmentRepo;
    private readonly Mock<IPatientRepository> _mockPatientRepo;
    private readonly Mock<ICaregiverRepository> _mockCaregiverRepo;
    private readonly Mock<ICaregiverScheduleRepository> _mockScheduleRepo;
    private readonly AppointmentService _service;

    public CompleteAppointmentTests()
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
    public async Task CompleteAppointmentAsync_ValidAppointmentAndCaregiver_ReturnsCompletedAppointment()
    {
        // Arrange
        var appointmentId = 1;
        var userId = 101;
        var request = new CompleteAppointmentRequest { CaregiverNotes = "All good." };

        // Mock: Caregiver exists linked to this UserId
        var caregiver = new Caregiver
        {
            Id = 1,
            UserId = userId,
            FirstName = "Test",
            LastName = "Caregiver",
            Specialisation = "General",
            Room = "101"
        };
        _mockCaregiverRepo.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(caregiver);

        // Mock: Appointment exists
        var appointment = new Appointment
        {
            Id = appointmentId,
            CaregiverId = 1,
            PatientId = 100, // Added
            Status = AppointmentStatus.Scheduled,
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            StartTime = new TimeOnly(09, 00), // Added
            EndTime = new TimeOnly(09, 30)
        };
        _mockAppointmentRepo.Setup(r => r.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);

        // Mock: Update succeeds
        _mockAppointmentRepo.Setup(r => r.UpdateAsync(It.IsAny<Appointment>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CompleteAppointmentAsync(appointmentId, userId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(AppointmentStatus.Completed, result.Status);
        Assert.Equal("All good.", result.CaregiverNotes);

        // Verify update was called
        _mockAppointmentRepo.Verify(r => r.UpdateAsync(It.Is<Appointment>(a =>
            a.Status == AppointmentStatus.Completed &&
            a.CaregiverNotes == "All good.")), Times.Once);
    }

    [Fact]
    public async Task CompleteAppointmentAsync_AppointmentNotFound_ThrowsAppointmentNotFoundException()
    {
        // Arrange
        var appointmentId = 999;
        var userId = 101;
        var request = new CompleteAppointmentRequest();

        // Mock: Caregiver exists
        var caregiver = new Caregiver
        {
            Id = 1,
            UserId = userId,
            FirstName = "Test",
            LastName = "Caregiver",
            Specialisation = "General",
            Room = "101"
        };
        _mockCaregiverRepo.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(caregiver);

        // Mock: Appointment does NOT exist
        _mockAppointmentRepo.Setup(r => r.GetByIdAsync(appointmentId))
            .ReturnsAsync((Appointment?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppointmentNotFoundException>(
            () => _service.CompleteAppointmentAsync(appointmentId, userId, request));

        Assert.Equal($"Appointment with ID {appointmentId} not found.", exception.Message);
    }

    #endregion

    #region Business Logic Tests

    [Fact]
    public async Task CompleteAppointmentAsync_WrongCaregiver_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var appointmentId = 1;
        var userId = 101; // Caregiver 1
        var request = new CompleteAppointmentRequest();

        // Mock: Caregiver exists (ID 1)
        var caregiver = new Caregiver
        {
            Id = 1,
            UserId = userId,
            FirstName = "Test",
            LastName = "Caregiver",
            Specialisation = "General",
            Room = "101"
        };
        _mockCaregiverRepo.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(caregiver);

        // Mock: Appointment exists but belongs to Caregiver 2
        var appointment = new Appointment
        {
            Id = appointmentId,
            CaregiverId = 2, // Different caregiver!
            PatientId = 100,
            Status = AppointmentStatus.Scheduled,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(10, 30)
        };
        _mockAppointmentRepo.Setup(r => r.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.CompleteAppointmentAsync(appointmentId, userId, request));

        Assert.Equal("You are not authorized to complete this appointment.", exception.Message);
    }

    [Fact]
    public async Task CompleteAppointmentAsync_BeforeEndTime_ThrowsAppointmentValidationException()
    {
        // Arrange
        var appointmentId = 1;
        var userId = 101;

        // Mock: Caregiver
        var caregiver = new Caregiver
        {
            Id = 1,
            UserId = userId,
            FirstName = "Test",
            LastName = "Caregiver",
            Specialisation = "General",
            Room = "101"
        };
        _mockCaregiverRepo.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(caregiver);

        // Mock: Appointment is in the FUTURE
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var appointment = new Appointment
        {
            Id = appointmentId,
            CaregiverId = 1,
            PatientId = 100,
            Status = AppointmentStatus.Scheduled,
            Date = futureDate,
            StartTime = new TimeOnly(9, 30),
            EndTime = new TimeOnly(10, 00)
        };
        _mockAppointmentRepo.Setup(r => r.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppointmentValidationException>(
            () => _service.CompleteAppointmentAsync(appointmentId, userId, new CompleteAppointmentRequest()));

        Assert.Equal("Cannot complete an appointment before its end time.", exception.Message);
    }

    [Fact]
    public async Task CompleteAppointmentAsync_AlreadyCompleted_ThrowsAppointmentValidationException()
    {
        // Arrange
        var appointmentId = 1;
        var userId = 101;

        // Mock: Caregiver
        var caregiver = new Caregiver
        {
            Id = 1,
            UserId = userId,
            FirstName = "Test",
            LastName = "Caregiver",
            Specialisation = "General",
            Room = "101"
        };
        _mockCaregiverRepo.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(caregiver);

        // Mock: Appointment is ALREADY completed
        var appointment = new Appointment
        {
            Id = appointmentId,
            CaregiverId = 1,
            PatientId = 100,
            Status = AppointmentStatus.Completed, // Not scheduled!
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            StartTime = new TimeOnly(9, 30),
            EndTime = new TimeOnly(10, 00)
        };
        _mockAppointmentRepo.Setup(r => r.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppointmentValidationException>(
            () => _service.CompleteAppointmentAsync(appointmentId, userId, new CompleteAppointmentRequest()));

        Assert.Contains("must be in 'Scheduled' status", exception.Message);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task CompleteAppointmentAsync_VeryLongCaregiverNotes_SavesSuccessfully()
    {
        // Arrange
        var appointmentId = 1;
        var userId = 101;
        var longNotes = new string('A', 1000); // 1000 characters
        var request = new CompleteAppointmentRequest { CaregiverNotes = longNotes };

        // Mock: Caregiver
        var caregiver = new Caregiver
        {
            Id = 1,
            UserId = userId,
            FirstName = "Test",
            LastName = "Caregiver",
            Specialisation = "General",
            Room = "101"
        };
        _mockCaregiverRepo.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(caregiver);

        // Mock: Appointment
        var appointment = new Appointment
        {
            Id = appointmentId,
            CaregiverId = 1,
            PatientId = 100,
            Status = AppointmentStatus.Scheduled,
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            StartTime = new TimeOnly(9, 30),
            EndTime = new TimeOnly(10, 00)
        };
        _mockAppointmentRepo.Setup(r => r.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);

        // Act
        var result = await _service.CompleteAppointmentAsync(appointmentId, userId, request);

        // Assert
        Assert.Equal(longNotes, result.CaregiverNotes);
        _mockAppointmentRepo.Verify(r => r.UpdateAsync(It.Is<Appointment>(a => a.CaregiverNotes == longNotes)), Times.Once);
    }

    #endregion
}
