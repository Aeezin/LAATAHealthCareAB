using HealthCareAB_v1.Services.Implementations;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Exceptions;
using Xunit;
using Moq;

namespace HealthCareAB_v1.Tests.Services;

public class GetCaregiverScheduleTests
{
    private readonly Mock<ICaregiverScheduleRepository> _mockRepository;
    private readonly Mock<ICaregiverRepository> _mockCaregiverRepository;
    private readonly CaregiverScheduleService _service;

    public GetCaregiverScheduleTests()
    {
        _mockRepository = new Mock<ICaregiverScheduleRepository>();
        _mockCaregiverRepository = new Mock<ICaregiverRepository>();
        _service = new CaregiverScheduleService(
            _mockRepository.Object,
            _mockCaregiverRepository.Object);
    }

    #region GetByIdAsync - Validation Tests

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsSchedule()
    {
        // Arrange
        var scheduleId = 1;
        var expectedSchedule = new CaregiverSchedule
        {
            Id = scheduleId,
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            IsActive = true
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(scheduleId))
            .ReturnsAsync(expectedSchedule);

        // Act
        var result = await _service.GetByIdAsync(scheduleId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(scheduleId, result.Id);
        Assert.Equal(expectedSchedule.CaregiverId, result.CaregiverId);
        _mockRepository.Verify(r => r.GetByIdAsync(scheduleId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var invalidId = 999;
        _mockRepository
            .Setup(r => r.GetByIdAsync(invalidId))
            .ReturnsAsync((CaregiverSchedule?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CaregiverScheduleNotFoundException>(
            () => _service.GetByIdAsync(invalidId)
        );

        Assert.Contains("not found", exception.Message, StringComparison.OrdinalIgnoreCase);
        _mockRepository.Verify(r => r.GetByIdAsync(invalidId), Times.Once);
    }

    #endregion

    #region GetByCaregiverIdAsync - Validation Tests

    [Fact]
    public async Task GetByCaregiverIdAsync_WithValidCaregiverId_ReturnsSchedules()
    {
        // Arrange
        var caregiverId = 1;
        var expectedSchedules = new List<CaregiverSchedule>
        {
            new CaregiverSchedule
            {
                Id = 1,
                CaregiverId = caregiverId,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0),
                IsActive = true
            },
            new CaregiverSchedule
            {
                Id = 2,
                CaregiverId = caregiverId,
                DayOfWeek = DayOfWeek.Tuesday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0),
                IsActive = true
            }
        };

        _mockRepository
            .Setup(r => r.GetByCaregiverIdAsync(caregiverId))
            .ReturnsAsync(expectedSchedules);

        // Act
        var result = await _service.GetByCaregiverIdAsync(caregiverId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, s => Assert.Equal(caregiverId, s.CaregiverId));
        _mockRepository.Verify(r => r.GetByCaregiverIdAsync(caregiverId), Times.Once);
    }

    [Fact]
    public async Task GetByCaregiverIdAsync_WithNoSchedules_ReturnsEmptyList()
    {
        // Arrange
        var caregiverId = 1;
        _mockRepository
            .Setup(r => r.GetByCaregiverIdAsync(caregiverId))
            .ReturnsAsync(new List<CaregiverSchedule>());

        // Act
        var result = await _service.GetByCaregiverIdAsync(caregiverId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockRepository.Verify(r => r.GetByCaregiverIdAsync(caregiverId), Times.Once);
    }

    #endregion

    #region GetByCaregiverIdAsync - Edge Cases

    [Fact]
    public async Task GetByCaregiverIdAsync_WithMultipleSchedulesOnSameDay_ReturnsAll()
    {
        // Arrange
        var caregiverId = 1;
        var expectedSchedules = new List<CaregiverSchedule>
        {
            new CaregiverSchedule
            {
                Id = 1,
                CaregiverId = caregiverId,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(12, 0),
                IsActive = true
            },
            new CaregiverSchedule
            {
                Id = 2,
                CaregiverId = caregiverId,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(13, 0),
                EndTime = new TimeOnly(17, 0),
                IsActive = true
            }
        };

        _mockRepository
            .Setup(r => r.GetByCaregiverIdAsync(caregiverId))
            .ReturnsAsync(expectedSchedules);

        // Act
        var result = await _service.GetByCaregiverIdAsync(caregiverId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, s => Assert.Equal(DayOfWeek.Monday, s.DayOfWeek));
        _mockRepository.Verify(r => r.GetByCaregiverIdAsync(caregiverId), Times.Once);
    }

    #endregion
}