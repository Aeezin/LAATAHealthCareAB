using HealthCareAB_v1.Services.Implementations;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Exceptions;
using Xunit;
using Moq;

namespace HealthCareAB_v1.Tests.Services;

public class CreateCaregiverScheduleTests
{
    private readonly Mock<ICaregiverScheduleRepository> _mockScheduleRepository;
    private readonly Mock<ICaregiverRepository> _mockCaregiverRepository;
    private readonly CaregiverScheduleService _service;

    public CreateCaregiverScheduleTests()
    {
        _mockScheduleRepository = new Mock<ICaregiverScheduleRepository>();
        _mockCaregiverRepository = new Mock<ICaregiverRepository>();

        _service = new CaregiverScheduleService(
            _mockScheduleRepository.Object,
            _mockCaregiverRepository.Object
        );
    }

    // POSITIVE VALIDATION TESTS

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsCreatedSchedule()
    {
        // Arrange
        var schedule = new CaregiverSchedule
        {
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(12, 0)
        };

        var expectedSchedule = new CaregiverSchedule
        {
            Id = 1,
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(12, 0),
            IsActive = true
        };

        // Mock: Caregiver exists
        _mockCaregiverRepository
            .Setup(repo => repo.ExistsAsync(1))
            .ReturnsAsync(true);

        // Mock: No overlap
        _mockScheduleRepository
            .Setup(repo => repo.HasOverlappingScheduleAsync(1, DayOfWeek.Monday,
                new TimeOnly(8, 0), new TimeOnly(12, 0)))
            .ReturnsAsync(false);

        // Mock: Create returns expected schedule
        _mockScheduleRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<CaregiverSchedule>()))
            .ReturnsAsync(expectedSchedule);

        // Act
        var result = await _service.CreateAsync(schedule);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(1, result.CaregiverId);
        Assert.Equal(DayOfWeek.Monday, result.DayOfWeek);
        Assert.True(result.IsActive);

        _mockScheduleRepository.Verify(repo => repo.CreateAsync(It.IsAny<CaregiverSchedule>()), Times.Once);
    }

    // NEGATIVE VALIDATION TESTS

    [Fact]
    public async Task CreateAsync_WithInvalidCaregiverId_ThrowsNotFoundException()
    {
        // Arrange
        var schedule = new CaregiverSchedule
        {
            CaregiverId = 999,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(12, 0)
        };

        // Mock: Caregiver does not exist
        _mockCaregiverRepository
            .Setup(repo => repo.ExistsAsync(999))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CaregiverScheduleNotFoundException>(
            () => _service.CreateAsync(schedule)
        );

        Assert.Contains("Caregiver with ID 999 not found", exception.Message);

        // Verify CreateAsync was never called
        _mockScheduleRepository.Verify(repo => repo.CreateAsync(It.IsAny<CaregiverSchedule>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithWeekendDay_ThrowsValidationException()
    {
        // Arrange - Saturday
        var schedule = new CaregiverSchedule
        {
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Saturday,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(12, 0)
        };

        // Mock: Caregiver exists (validation runs before weekday check)
        _mockCaregiverRepository
            .Setup(repo => repo.ExistsAsync(1))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CaregiverScheduleValidationException>(
            () => _service.CreateAsync(schedule)
        );

        Assert.Equal("Schedules can only be created for weekdays (Monday-Friday).", exception.Message);

        _mockScheduleRepository.Verify(repo => repo.CreateAsync(It.IsAny<CaregiverSchedule>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithSunday_ThrowsValidationException()
    {
        // Arrange - Sunday
        var schedule = new CaregiverSchedule
        {
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Sunday,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(12, 0)
        };

        // Mock: Caregiver exists
        _mockCaregiverRepository
            .Setup(repo => repo.ExistsAsync(1))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CaregiverScheduleValidationException>(
            () => _service.CreateAsync(schedule)
        );

        Assert.Equal("Schedules can only be created for weekdays (Monday-Friday).", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_WithStartTimeAfterEndTime_ThrowsValidationException()
    {
        // Arrange
        var schedule = new CaregiverSchedule
        {
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(12, 0),
            EndTime = new TimeOnly(8, 0)
        };

        // Mock: Caregiver exists
        _mockCaregiverRepository
            .Setup(repo => repo.ExistsAsync(1))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CaregiverScheduleValidationException>(
            () => _service.CreateAsync(schedule)
        );

        Assert.Equal("StartTime must be before EndTime.", exception.Message);

        _mockScheduleRepository.Verify(repo => repo.CreateAsync(It.IsAny<CaregiverSchedule>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithStartTimeEqualToEndTime_ThrowsValidationException()
    {
        // Arrange
        var schedule = new CaregiverSchedule
        {
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(8, 0)
        };

        // Mock: Caregiver exists
        _mockCaregiverRepository
            .Setup(repo => repo.ExistsAsync(1))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CaregiverScheduleValidationException>(
            () => _service.CreateAsync(schedule)
        );

        Assert.Equal("StartTime must be before EndTime.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_WithOverlappingSchedule_ThrowsValidationException()
    {
        // Arrange
        var schedule = new CaregiverSchedule
        {
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(14, 0)
        };

        // Mock: Caregiver exists
        _mockCaregiverRepository
            .Setup(repo => repo.ExistsAsync(1))
            .ReturnsAsync(true);

        // Mock: Overlap exists
        _mockScheduleRepository
            .Setup(repo => repo.HasOverlappingScheduleAsync(1, DayOfWeek.Monday,
                new TimeOnly(10, 0), new TimeOnly(14, 0)))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CaregiverScheduleValidationException>(
            () => _service.CreateAsync(schedule)
        );

        Assert.Contains("overlaps with an existing schedule", exception.Message);

        _mockScheduleRepository.Verify(repo => repo.CreateAsync(It.IsAny<CaregiverSchedule>()), Times.Never);
    }

    // EDGE CASES - POSITIVE

    [Fact]
    public async Task CreateAsync_ForDifferentCaregivers_WithSameTime_Succeeds()
    {
        // Arrange
        var scheduleCaregiver2 = new CaregiverSchedule
        {
            CaregiverId = 2,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(12, 0)
        };

        var expectedSchedule = new CaregiverSchedule
        {
            Id = 2,
            CaregiverId = 2,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(12, 0),
            IsActive = true
        };

        // Mock: Caregiver 2 exists
        _mockCaregiverRepository
            .Setup(repo => repo.ExistsAsync(2))
            .ReturnsAsync(true);

        // Mock: No overlap for caregiver 2
        _mockScheduleRepository
            .Setup(repo => repo.HasOverlappingScheduleAsync(2, DayOfWeek.Monday,
                new TimeOnly(8, 0), new TimeOnly(12, 0)))
            .ReturnsAsync(false);

        _mockScheduleRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<CaregiverSchedule>()))
            .ReturnsAsync(expectedSchedule);

        // Act
        var result = await _service.CreateAsync(scheduleCaregiver2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.CaregiverId);
        _mockScheduleRepository.Verify(repo => repo.CreateAsync(It.IsAny<CaregiverSchedule>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithSameDayButDifferentTimes_Succeeds()
    {
        // Arrange
        var afternoonSchedule = new CaregiverSchedule
        {
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(13, 0),
            EndTime = new TimeOnly(17, 0)
        };

        var expectedSchedule = new CaregiverSchedule
        {
            Id = 2,
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(13, 0),
            EndTime = new TimeOnly(17, 0),
            IsActive = true
        };

        // Mock: Caregiver exists
        _mockCaregiverRepository
            .Setup(repo => repo.ExistsAsync(1))
            .ReturnsAsync(true);

        // Mock: No overlap
        _mockScheduleRepository
            .Setup(repo => repo.HasOverlappingScheduleAsync(1, DayOfWeek.Monday,
                new TimeOnly(13, 0), new TimeOnly(17, 0)))
            .ReturnsAsync(false);

        _mockScheduleRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<CaregiverSchedule>()))
            .ReturnsAsync(expectedSchedule);

        // Act
        var result = await _service.CreateAsync(afternoonSchedule);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DayOfWeek.Monday, result.DayOfWeek);
        _mockScheduleRepository.Verify(repo => repo.CreateAsync(It.IsAny<CaregiverSchedule>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_SameCaregiver_DifferentDays_Succeeds()
    {
        // Arrange
        var tuesdaySchedule = new CaregiverSchedule
        {
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Tuesday,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(12, 0)
        };

        var expectedSchedule = new CaregiverSchedule
        {
            Id = 2,
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Tuesday,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(12, 0),
            IsActive = true
        };

        // Mock: Caregiver exists
        _mockCaregiverRepository
            .Setup(repo => repo.ExistsAsync(1))
            .ReturnsAsync(true);

        // Mock: No overlap
        _mockScheduleRepository
            .Setup(repo => repo.HasOverlappingScheduleAsync(1, DayOfWeek.Tuesday,
                new TimeOnly(8, 0), new TimeOnly(12, 0)))
            .ReturnsAsync(false);

        _mockScheduleRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<CaregiverSchedule>()))
            .ReturnsAsync(expectedSchedule);

        // Act
        var result = await _service.CreateAsync(tuesdaySchedule);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DayOfWeek.Tuesday, result.DayOfWeek);
        _mockScheduleRepository.Verify(repo => repo.CreateAsync(It.IsAny<CaregiverSchedule>()), Times.Once);
    }

    // BUSINESS LOGIC TESTS

    [Fact]
    public async Task CreateAsync_CallsRepositoryCreateAsync_ExactlyOnce()
    {
        // Arrange
        var schedule = new CaregiverSchedule
        {
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(12, 0)
        };

        // Mock: Caregiver exists
        _mockCaregiverRepository
            .Setup(repo => repo.ExistsAsync(1))
            .ReturnsAsync(true);

        // Mock: No overlap
        _mockScheduleRepository
            .Setup(repo => repo.HasOverlappingScheduleAsync(It.IsAny<int>(), It.IsAny<DayOfWeek>(),
                It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>()))
            .ReturnsAsync(false);

        _mockScheduleRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<CaregiverSchedule>()))
            .ReturnsAsync(schedule);

        // Act
        await _service.CreateAsync(schedule);

        // Assert
        _mockScheduleRepository.Verify(
            repo => repo.CreateAsync(It.Is<CaregiverSchedule>(s =>
                s.CaregiverId == 1 &&
                s.DayOfWeek == DayOfWeek.Monday)),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateAsync_ChecksOverlap_BeforeCreating()
    {
        // Arrange
        var schedule = new CaregiverSchedule
        {
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(12, 0)
        };

        var callOrder = new List<string>();

        // Mock: Caregiver exists
        _mockCaregiverRepository
            .Setup(repo => repo.ExistsAsync(1))
            .ReturnsAsync(true);

        _mockScheduleRepository
            .Setup(repo => repo.HasOverlappingScheduleAsync(It.IsAny<int>(), It.IsAny<DayOfWeek>(),
                It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>()))
            .ReturnsAsync(false)
            .Callback(() => callOrder.Add("HasOverlap"));

        _mockScheduleRepository
            .Setup(repo => repo.CreateAsync(It.IsAny<CaregiverSchedule>()))
            .ReturnsAsync(schedule)
            .Callback(() => callOrder.Add("Create"));

        // Act
        await _service.CreateAsync(schedule);

        // Assert
        Assert.Equal(2, callOrder.Count);
        Assert.Equal("HasOverlap", callOrder[0]);
        Assert.Equal("Create", callOrder[1]);
    }
}