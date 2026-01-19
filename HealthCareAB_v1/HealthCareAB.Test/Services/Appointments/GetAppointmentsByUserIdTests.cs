using HealthCareAB_v1.Services.Implementations;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Models.Enums;
using HealthCareAB_v1.Exceptions;
using Xunit;
using Moq;

namespace HealthCareAB_v1.Tests.Appointments;

public class GetAppointmentsByUserIdTests
{
    private readonly Mock<IAppointmentRepository> _mockAppointmentRepo;
    private readonly Mock<IPatientRepository> _mockPatientRepo;
    private readonly Mock<ICaregiverRepository> _mockCaregiverRepo;
    private readonly Mock<ICaregiverScheduleRepository> _mockScheduleRepo;
    private readonly AppointmentService _service;

    public GetAppointmentsByUserIdTests()
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

    // POSITIVE: Patient can get their appointments
    [Fact]
    public async Task GetByUserIdAsync_AsPatient_ReturnsPatientAppointments()
    {
        // Arrange
        int userId = 1;
        int patientId = 10;

        var patient = new Patient
        {
            Id = patientId,
            UserId = userId,
            FirstName = "Johan",
            LastName = "Andersson",
            PersonalIdentityNumber = "199001011234",
            PhoneNumber = "0701234567",
            DateOfBirth = "1990-01-01"
        };

        var appointments = new List<Appointment>
        {
            new Appointment
            {
                Id = 1,
                PatientId = patientId,
                CaregiverId = 5,
                Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(11, 0),
                Status = AppointmentStatus.Scheduled
            }
        };

        _mockPatientRepo
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(patient);

        _mockAppointmentRepo
            .Setup(r => r.GetByPatientIdAsync(patientId))
            .ReturnsAsync(appointments);

        // Act
        var result = await _service.GetByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(patientId, result[0].PatientId);
    }

    // POSITIVE: Caregiver can get their appointments
    [Fact]
    public async Task GetByUserIdAsync_AsCaregiver_ReturnsCaregiverAppointments()
    {
        // Arrange
        int userId = 2;
        int caregiverId = 20;

        var caregiver = new Caregiver
        {
            Id = caregiverId,
            UserId = userId,
            FirstName = "Dr. Maria",
            LastName = "Svensson",
            Specialisation = "Cardiologist",
            Room = "A101",
            Verified = true,
            IsAcceptingPatients = true
        };

        var appointments = new List<Appointment>
        {
            new Appointment
            {
                Id = 2,
                PatientId = 10,
                CaregiverId = caregiverId,
                Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
                StartTime = new TimeOnly(14, 0),
                EndTime = new TimeOnly(15, 0),
                Status = AppointmentStatus.Scheduled
            }
        };

        _mockPatientRepo
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((Patient?)null);

        _mockCaregiverRepo
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(caregiver);

        _mockAppointmentRepo
            .Setup(r => r.GetByCaregiverIdAsync(caregiverId))
            .ReturnsAsync(appointments);

        // Act
        var result = await _service.GetByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(caregiverId, result[0].CaregiverId);
    }

    // NEGATIVE: User profile not found throws exception
    [Fact]
    public async Task GetByUserIdAsync_UserNotFound_ThrowsNotFoundException()
    {
        // Arrange
        int userId = 999;

        _mockPatientRepo
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((Patient?)null);

        _mockCaregiverRepo
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync((Caregiver?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _service.GetByUserIdAsync(userId)
        );

        Assert.Equal("User profile not found", exception.Message);
    }

    // EDGE CASE: User with no appointments returns empty list
    [Fact]
    public async Task GetByUserIdAsync_NoAppointments_ReturnsEmptyList()
    {
        // Arrange
        int userId = 1;
        int patientId = 10;

        var patient = new Patient
        {
            Id = patientId,
            UserId = userId,
            FirstName = "Johan",
            LastName = "Andersson",
            PersonalIdentityNumber = "199001011234",
            PhoneNumber = "0701234567",
            DateOfBirth = "1990-01-01"
        };

        _mockPatientRepo
            .Setup(r => r.GetByUserIdAsync(userId))
            .ReturnsAsync(patient);

        _mockAppointmentRepo
            .Setup(r => r.GetByPatientIdAsync(patientId))
            .ReturnsAsync(new List<Appointment>());

        // Act
        var result = await _service.GetByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}