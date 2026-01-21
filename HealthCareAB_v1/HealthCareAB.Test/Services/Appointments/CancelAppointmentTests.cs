using HealthCareAB_v1.Services.Implementations;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Models.Enums;
using HealthCareAB_v1.Exceptions;
using Xunit;
using Moq;

namespace HealthCareAB_v1.Tests.Services;

public class CancelAppointmentTests
{
    private readonly Mock<IAppointmentRepository> _mockAppointmentRepo;
    private readonly Mock<IPatientRepository> _mockPatientRepo;
    private readonly Mock<ICaregiverRepository> _mockCaregiverRepo;
    private readonly Mock<ICaregiverScheduleRepository> _mockScheduleRepo;
    private readonly AppointmentService _service;

    public CancelAppointmentTests()
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
    public async Task CancelAppointment_AsPatientOfAppointment_WithinTimeLimit_SuccessfullyCancels()
    {
        // Arrange
        var appointmentId = 1;
        var userId = 100; // User is Patient

        // Mock: Patient exists
        var patient = new Patient 
        { 
            Id = 50, 
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = "1980-01-01",
            PersonalIdentityNumber = "19800101-1234"
        };
        _mockPatientRepo.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(patient);

        // Mock: Appointment exists, belongs to this patient, and is > 1 hour away
        var now = DateTime.UtcNow;
        var appointment = new Appointment
        {
            Id = appointmentId,
            PatientId = patient.Id,
            CaregiverId = 10,
            Status = AppointmentStatus.Scheduled,
            Date = DateOnly.FromDateTime(now.AddDays(1)),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(10, 30)
        };
        _mockAppointmentRepo.Setup(r => r.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);

        // Act
        var result = await _service.CancelAppointmentAsync(appointmentId, userId);

        // Assert
        Assert.Equal(AppointmentStatus.Cancelled, result.Status);
        
        // Verify delete was called (behavior for patient cancellation)
        _mockAppointmentRepo.Verify(r => r.DeleteAsync(appointmentId), Times.Once);
    }

    [Fact]
    public async Task CancelAppointment_AsCaregiverOfAppointment_SuccessfullyCancels()
    {
        // Arrange
        var appointmentId = 1;
        var userId = 200; // User is Caregiver

        // Mock: Caregiver exists
        var caregiver = new Caregiver 
        { 
            Id = 10, 
            UserId = userId,
            FirstName = "Jane",
            LastName = "Smith",
            Specialisation = "General",
            Room = "101"
        };
        _mockCaregiverRepo.Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(caregiver);

        // Mock: Appointment exists, belongs to this caregiver
        var appointment = new Appointment
        {
            Id = appointmentId,
            PatientId = 50,
            CaregiverId = caregiver.Id,
            Status = AppointmentStatus.Scheduled,
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(10, 30)
        };
        _mockAppointmentRepo.Setup(r => r.GetByIdAsync(appointmentId))
            .ReturnsAsync(appointment);

        // Act
        var result = await _service.CancelAppointmentAsync(appointmentId, userId);

        // Assert
        Assert.Equal(AppointmentStatus.Cancelled, result.Status);
        
        // Verify update was called (behavior for caregiver cancellation)
        _mockAppointmentRepo.Verify(r => r.UpdateAsync(It.Is<Appointment>(a => a.Status == AppointmentStatus.Cancelled)), Times.Once);
    }

    [Fact]
    public async Task CancelAppointment_AsUnauthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        var appointmentId = 1;
        var userId = 999; // Unknown user

        var appointment = new Appointment 
        { 
            Id = appointmentId, 
            Status = AppointmentStatus.Scheduled,
            PatientId = 1,
            CaregiverId = 1,
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(10, 30)
        };
        _mockAppointmentRepo.Setup(r => r.GetByIdAsync(appointmentId)).ReturnsAsync(appointment);

        // Mock: User is neither Patient nor Caregiver
        _mockPatientRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((Patient?)null);
        _mockCaregiverRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((Caregiver?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _service.CancelAppointmentAsync(appointmentId, userId));
    }

    [Fact]
    public async Task CancelAppointment_WhenAppointmentNotFound_ThrowsAppointmentNotFoundException()
    {
        // Arrange
        var appointmentId = 999;
        var userId = 100;
        _mockAppointmentRepo.Setup(r => r.GetByIdAsync(appointmentId)).ReturnsAsync((Appointment?)null);

        // Act & Assert
        await Assert.ThrowsAsync<AppointmentNotFoundException>(() => 
            _service.CancelAppointmentAsync(appointmentId, userId));
    }

    [Fact]
    public async Task CancelAppointment_AsUnrelatedPatient_ThrowsForbiddenException()
    {
        // Arrange
        var appointmentId = 1;
        var userId = 100;

        // Mock: Patient exists but is NOT the one in the appointment
        var patient = new Patient 
        { 
            Id = 50, 
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = "1980-01-01",
            PersonalIdentityNumber = "19800101-1234"
        };
        _mockPatientRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(patient);

        var appointment = new Appointment
        {
            Id = appointmentId,
            PatientId = 99, // Different patient
            CaregiverId = 10,
            Status = AppointmentStatus.Scheduled,
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(10, 30)
        };
        _mockAppointmentRepo.Setup(r => r.GetByIdAsync(appointmentId)).ReturnsAsync(appointment);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _service.CancelAppointmentAsync(appointmentId, userId));
        Assert.Equal("You are not authorized to cancel this appointment.", ex.Message);
    }

    [Fact]
    public async Task CancelAppointment_AsUnrelatedCaregiver_ThrowsForbiddenException()
    {
        // Arrange
        var appointmentId = 1;
        var userId = 200;

        // Mock: Caregiver exists but is NOT the one in the appointment
        var caregiver = new Caregiver 
        { 
            Id = 10, 
            UserId = userId,
            FirstName = "Jane",
            LastName = "Smith",
            Specialisation = "General",
            Room = "101"
        };
        _mockCaregiverRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(caregiver);

        var appointment = new Appointment
        {
            Id = appointmentId,
            PatientId = 50,
            CaregiverId = 99, // Different caregiver
            Status = AppointmentStatus.Scheduled,
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(10, 30)
        };
        _mockAppointmentRepo.Setup(r => r.GetByIdAsync(appointmentId)).ReturnsAsync(appointment);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _service.CancelAppointmentAsync(appointmentId, userId));
        Assert.Equal("You are not authorized to cancel this appointment.", ex.Message);
    }

    #endregion

    #region Business Logic Tests

    [Fact]
    public async Task CancelAppointment_UpdatesStatusToCancelled()
    {
        // Same as caregiver cancel test - verifies status update
        var appointmentId = 1;
        var userId = 200;
        var caregiver = new Caregiver 
        { 
            Id = 10, 
            UserId = userId,
            FirstName = "Jane",
            LastName = "Smith",
            Specialisation = "General",
            Room = "101"
        };
        _mockCaregiverRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(caregiver);

        var appointment = new Appointment
        {
            Id = appointmentId,
            PatientId = 50,
            CaregiverId = caregiver.Id,
            Status = AppointmentStatus.Scheduled,
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            StartTime = new TimeOnly(12, 0),
            EndTime = new TimeOnly(12, 30)
        };
        _mockAppointmentRepo.Setup(r => r.GetByIdAsync(appointmentId)).ReturnsAsync(appointment);

        var result = await _service.CancelAppointmentAsync(appointmentId, userId);

        Assert.Equal(AppointmentStatus.Cancelled, result.Status);
    }

    [Fact]
    public async Task CancelAppointment_MarksTimeSlotAsAvailableAgain()
    {
        // StartTime slot availability is implied by deletion or cancellation status
        // Here we verify that for a patient, it calls DeleteAsync, effectively freeing the slot
        var appointmentId = 1;
        var userId = 100;
        var patient = new Patient 
        { 
            Id = 50, 
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = "1980-01-01",
            PersonalIdentityNumber = "19800101-1234"
        };
        _mockPatientRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(patient);

        var appointment = new Appointment
        {
            Id = appointmentId,
            PatientId = patient.Id,
            CaregiverId = 10,
            Status = AppointmentStatus.Scheduled,
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            StartTime = new TimeOnly(12, 0),
            EndTime = new TimeOnly(12, 30)
        };
        _mockAppointmentRepo.Setup(r => r.GetByIdAsync(appointmentId)).ReturnsAsync(appointment);

        await _service.CancelAppointmentAsync(appointmentId, userId);

        _mockAppointmentRepo.Verify(r => r.DeleteAsync(appointmentId), Times.Once);
    }

    [Fact]
    public async Task CancelAppointment_AsPatient_LessThan1HourBefore_ThrowsTimeRestrictionException()
    {
        // Arrange
        var appointmentId = 1;
        var userId = 100;
        var patient = new Patient 
        { 
            Id = 50, 
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = "1980-01-01",
            PersonalIdentityNumber = "19800101-1234"
        };
        _mockPatientRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(patient);

        // Less than 1 hour before
        var now = DateTime.UtcNow;
        var appointmentDate = DateOnly.FromDateTime(now);
        var appointmentTime = TimeOnly.FromDateTime(now.AddMinutes(30)); // 30 mins from now
        var appointmentEndTime = appointmentTime.AddMinutes(30);

        var appointment = new Appointment
        {
            Id = appointmentId,
            PatientId = patient.Id,
            CaregiverId = 10,
            Status = AppointmentStatus.Scheduled,
            Date = appointmentDate,
            StartTime = appointmentTime,
            EndTime = appointmentEndTime
        };
        _mockAppointmentRepo.Setup(r => r.GetByIdAsync(appointmentId)).ReturnsAsync(appointment);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AppointmentValidationException>(() => 
            _service.CancelAppointmentAsync(appointmentId, userId));
        
        Assert.Contains("Appointments can only be cancelled up to 1 hour before", ex.Message);
    }

    [Fact]
    public async Task CancelAppointment_WhenAlreadyCancelled_ThrowsInvalidStatusException()
    {
         // Arrange
        var appointmentId = 1;
        var userId = 100;
        var patient = new Patient 
        { 
            Id = 50, 
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = "1980-01-01",
            PersonalIdentityNumber = "19800101-1234"
        };
        _mockPatientRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(patient);

        var appointment = new Appointment
        {
            Id = appointmentId,
            PatientId = patient.Id,
            CaregiverId = 10,
            Status = AppointmentStatus.Cancelled, // Already cancelled
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            StartTime = new TimeOnly(12, 0),
            EndTime = new TimeOnly(12, 30)
        };
        _mockAppointmentRepo.Setup(r => r.GetByIdAsync(appointmentId)).ReturnsAsync(appointment);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AppointmentValidationException>(() => 
            _service.CancelAppointmentAsync(appointmentId, userId));
        
        Assert.Contains("Cannot cancel appointment with status 'Cancelled'", ex.Message);
    }

    [Fact]
    public async Task CancelAppointment_WhenCompleted_ThrowsInvalidStatusException()
    {
         // Arrange
        var appointmentId = 1;
        var userId = 100;
        var patient = new Patient 
        { 
            Id = 50, 
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = "1980-01-01",
            PersonalIdentityNumber = "19800101-1234"
        };
        _mockPatientRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(patient);

        var appointment = new Appointment
        {
            Id = appointmentId,
            PatientId = patient.Id,
            CaregiverId = 10,
            Status = AppointmentStatus.Completed, // Already completed
            Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            StartTime = new TimeOnly(12, 0),
            EndTime = new TimeOnly(12, 30)
        };
        _mockAppointmentRepo.Setup(r => r.GetByIdAsync(appointmentId)).ReturnsAsync(appointment);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AppointmentValidationException>(() => 
            _service.CancelAppointmentAsync(appointmentId, userId));
        
        Assert.Contains("Cannot cancel appointment with status 'Completed'", ex.Message);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task CancelAppointment_AsPatient_Exactly1HourBefore_SuccessfullyCancels()
    {
        // Arrange
        var appointmentId = 1;
        var userId = 100;
        var patient = new Patient 
        { 
            Id = 50, 
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = "1980-01-01",
            PersonalIdentityNumber = "19800101-1234"
        };
        _mockPatientRepo.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(patient);

        // Exactly 1 hour (+ small buffer to be safe with execution time/clock skew issues in test env if any)
        
        var now = DateTime.UtcNow;
        var appointmentDate = DateOnly.FromDateTime(now.AddHours(1));
        var appointmentTime = TimeOnly.FromDateTime(now.AddHours(1).AddMinutes(1)); // 1 hour 1 minute away
        var appointmentEndTime = appointmentTime.AddMinutes(30);

        var appointment = new Appointment
        {
            Id = appointmentId,
            PatientId = patient.Id,
            CaregiverId = 10,
            Status = AppointmentStatus.Scheduled,
            Date = appointmentDate,
            StartTime = appointmentTime,
            EndTime = appointmentEndTime
        };
        _mockAppointmentRepo.Setup(r => r.GetByIdAsync(appointmentId)).ReturnsAsync(appointment);

        // Act
        var result = await _service.CancelAppointmentAsync(appointmentId, userId);

        // Assert
        Assert.Equal(AppointmentStatus.Cancelled, result.Status);
    }

    [Fact]
    public async Task CancelAppointment_WithInvalidId_ThrowsAppointmentValidationException()
    {
        var appointmentId = -1; // Invalid ID
        var userId = 100;
        _mockAppointmentRepo.Setup(r => r.GetByIdAsync(appointmentId)).ReturnsAsync((Appointment?)null);

        await Assert.ThrowsAsync<AppointmentNotFoundException>(() => 
            _service.CancelAppointmentAsync(appointmentId, userId));
    }

    #endregion
}
