using HealthCareAB_v1.Services.Implementations;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services.Interfaces;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Models.DTOs;
using HealthCareAB_v1.Exceptions;
using Xunit;
using Moq;

namespace HealthCareAB_v1.Tests.Services;

public class UpdateCaregiverScheduleTests
{
    private readonly Mock<ICaregiverScheduleRepository> _mockRepository;
    private readonly Mock<ICaregiverRepository> _mockCaregiverRepository;
    private readonly CaregiverScheduleService _service;

    public UpdateCaregiverScheduleTests()
    {
        _mockRepository = new Mock<ICaregiverScheduleRepository>();
        _mockCaregiverRepository = new Mock<ICaregiverRepository>();
        _service = new CaregiverScheduleService(
            _mockRepository.Object,
            _mockCaregiverRepository.Object);
    }

    #region Validation Tests - Positive

    [Fact]
    public async Task UpdateSchedule_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var scheduleId = 1;
        var existingSchedule = new CaregiverSchedule
        {
            Id = scheduleId,
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            IsActive = true
        };

        var updateRequest = new UpdateCaregiverScheduleRequest
        {
            StartTime = new TimeOnly(10, 0)
        };

        _mockRepository.Setup(r => r.GetByIdAsync(scheduleId))
            .ReturnsAsync(existingSchedule);
        _mockRepository.Setup(r => r.HasOverlappingScheduleAsync(
                It.IsAny<int>(), It.IsAny<DayOfWeek>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), It.IsAny<int?>()))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<CaregiverSchedule>()))
            .ReturnsAsync((CaregiverSchedule s) => s);

        // Act
        var result = await _service.UpdateAsync(scheduleId, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(new TimeOnly(10, 0), result.StartTime);
        Assert.Equal(existingSchedule.DayOfWeek, result.DayOfWeek); // Unchanged
        Assert.Equal(existingSchedule.EndTime, result.EndTime); // Unchanged
    }

    #endregion

    #region Validation Tests - Negative

    [Theory]
    [InlineData(-1)]
    [InlineData(7)]
    [InlineData(8)]
    public async Task UpdateSchedule_WithInvalidDayOfWeek_ReturnsBadRequest(int invalidDayOfWeek)
    {
        // Arrange
        var scheduleId = 1;
        var existingSchedule = new CaregiverSchedule
        {
            Id = scheduleId,
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            IsActive = true
        };

        var updateRequest = new UpdateCaregiverScheduleRequest
        {
            DayOfWeek = (DayOfWeek)invalidDayOfWeek
        };

        _mockRepository.Setup(r => r.GetByIdAsync(scheduleId))
            .ReturnsAsync(existingSchedule);

        // Act & Assert
        await Assert.ThrowsAsync<CaregiverScheduleValidationException>(
            () => _service.UpdateAsync(scheduleId, updateRequest));
    }

    [Theory]
    [InlineData(0)] // Sunday
    [InlineData(6)] // Saturday
    public async Task UpdateSchedule_ChangeDayOfWeek_ToWeekend_ReturnsBadRequest(int weekendDay)
    {
        // Arrange
        var scheduleId = 1;
        var existingSchedule = new CaregiverSchedule
        {
            Id = scheduleId,
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            IsActive = true
        };

        var updateRequest = new UpdateCaregiverScheduleRequest
        {
            DayOfWeek = (DayOfWeek)weekendDay
        };

        _mockRepository.Setup(r => r.GetByIdAsync(scheduleId))
            .ReturnsAsync(existingSchedule);

        // Act & Assert
        await Assert.ThrowsAsync<CaregiverScheduleValidationException>(
            () => _service.UpdateAsync(scheduleId, updateRequest));
    }

    [Fact]
    public async Task UpdateSchedule_WithOverlappingSchedule_ReturnsBadRequest()
    {
        // Arrange
        var scheduleId = 1;
        var existingSchedule = new CaregiverSchedule
        {
            Id = scheduleId,
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            IsActive = true
        };

        var updateRequest = new UpdateCaregiverScheduleRequest
        {
            StartTime = new TimeOnly(8, 0)
        };

        _mockRepository.Setup(r => r.GetByIdAsync(scheduleId))
            .ReturnsAsync(existingSchedule);
        _mockRepository.Setup(r => r.HasOverlappingScheduleAsync(
                It.IsAny<int>(), It.IsAny<DayOfWeek>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), It.IsAny<int?>()))
            .ReturnsAsync(true); // Simulate overlap

        // Act & Assert
        await Assert.ThrowsAsync<CaregiverScheduleValidationException>(
            () => _service.UpdateAsync(scheduleId, updateRequest));
    }

    [Fact]
    public async Task UpdateSchedule_WithInvalidScheduleId_ReturnsNotFound()
    {
        // Arrange
        var scheduleId = 999;
        var updateRequest = new UpdateCaregiverScheduleRequest
        {
            StartTime = new TimeOnly(10, 0)
        };

        _mockRepository.Setup(r => r.GetByIdAsync(scheduleId))
            .ReturnsAsync((CaregiverSchedule?)null); // Schedule not found

        // Act & Assert
        await Assert.ThrowsAsync<CaregiverScheduleNotFoundException>(
            () => _service.UpdateAsync(scheduleId, updateRequest));
    }

    [Fact]
    public async Task UpdateSchedule_WithStartTimeAfterEndTime_ReturnsBadRequest()
    {
        // Arrange
        var scheduleId = 1;
        var existingSchedule = new CaregiverSchedule
        {
            Id = scheduleId,
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            IsActive = true
        };

        var updateRequest = new UpdateCaregiverScheduleRequest
        {
            StartTime = new TimeOnly(18, 0) // After existing end time
        };

        _mockRepository.Setup(r => r.GetByIdAsync(scheduleId))
            .ReturnsAsync(existingSchedule);

        // Act & Assert
        await Assert.ThrowsAsync<CaregiverScheduleValidationException>(
            () => _service.UpdateAsync(scheduleId, updateRequest));
    }

    [Fact]
    public async Task UpdateSchedule_WithEndTimeBeforeStartTime_ReturnsBadRequest()
    {
        // Arrange
        var scheduleId = 1;
        var existingSchedule = new CaregiverSchedule
        {
            Id = scheduleId,
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            IsActive = true
        };

        var updateRequest = new UpdateCaregiverScheduleRequest
        {
            EndTime = new TimeOnly(8, 0) // Before existing start time
        };

        _mockRepository.Setup(r => r.GetByIdAsync(scheduleId))
            .ReturnsAsync(existingSchedule);

        // Act & Assert
        await Assert.ThrowsAsync<CaregiverScheduleValidationException>(
            () => _service.UpdateAsync(scheduleId, updateRequest));
    }

    #endregion

    #region Business Logic Tests - Positive

    [Theory]
    [InlineData("StartTime")]
    [InlineData("EndTime")]
    [InlineData("DayOfWeek")]
    [InlineData("IsActive")]
    public async Task UpdateSchedule_UpdatesSingleField_Successfully(string fieldToUpdate)
    {
        // Arrange
        var scheduleId = 1;
        var existingSchedule = new CaregiverSchedule
        {
            Id = scheduleId,
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            IsActive = true
        };

        UpdateCaregiverScheduleRequest updateRequest = fieldToUpdate switch
        {
            "StartTime" => new UpdateCaregiverScheduleRequest { StartTime = new TimeOnly(10, 0) },
            "EndTime" => new UpdateCaregiverScheduleRequest { EndTime = new TimeOnly(18, 0) },
            "DayOfWeek" => new UpdateCaregiverScheduleRequest { DayOfWeek = DayOfWeek.Tuesday },
            "IsActive" => new UpdateCaregiverScheduleRequest { IsActive = false },
            _ => throw new ArgumentException("Invalid field")
        };

        _mockRepository.Setup(r => r.GetByIdAsync(scheduleId))
            .ReturnsAsync(existingSchedule);
        _mockRepository.Setup(r => r.HasOverlappingScheduleAsync(
                It.IsAny<int>(), It.IsAny<DayOfWeek>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), It.IsAny<int?>()))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<CaregiverSchedule>()))
            .ReturnsAsync((CaregiverSchedule s) => s);

        // Act
        var result = await _service.UpdateAsync(scheduleId, updateRequest);

        // Assert
        Assert.NotNull(result);
        switch (fieldToUpdate)
        {
            case "StartTime":
                Assert.Equal(new TimeOnly(10, 0), result.StartTime);
                Assert.Equal(existingSchedule.EndTime, result.EndTime);
                Assert.Equal(existingSchedule.DayOfWeek, result.DayOfWeek);
                Assert.Equal(existingSchedule.IsActive, result.IsActive);
                break;
            case "EndTime":
                Assert.Equal(new TimeOnly(18, 0), result.EndTime);
                Assert.Equal(existingSchedule.StartTime, result.StartTime);
                Assert.Equal(existingSchedule.DayOfWeek, result.DayOfWeek);
                Assert.Equal(existingSchedule.IsActive, result.IsActive);
                break;
            case "DayOfWeek":
                Assert.Equal(DayOfWeek.Tuesday, result.DayOfWeek);
                Assert.Equal(existingSchedule.StartTime, result.StartTime);
                Assert.Equal(existingSchedule.EndTime, result.EndTime);
                Assert.Equal(existingSchedule.IsActive, result.IsActive);
                break;
            case "IsActive":
                Assert.False(result.IsActive);
                Assert.Equal(existingSchedule.StartTime, result.StartTime);
                Assert.Equal(existingSchedule.EndTime, result.EndTime);
                Assert.Equal(existingSchedule.DayOfWeek, result.DayOfWeek);
                break;
        }
    }

    [Fact]
    public async Task UpdateSchedule_UpdatesMultipleFields_Simultaneously()
    {
        // Arrange
        var scheduleId = 1;
        var existingSchedule = new CaregiverSchedule
        {
            Id = scheduleId,
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            IsActive = true
        };

        var updateRequest = new UpdateCaregiverScheduleRequest
        {
            DayOfWeek = DayOfWeek.Tuesday,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(18, 0),
            IsActive = false
        };

        _mockRepository.Setup(r => r.GetByIdAsync(scheduleId))
            .ReturnsAsync(existingSchedule);
        _mockRepository.Setup(r => r.HasOverlappingScheduleAsync(
                It.IsAny<int>(), It.IsAny<DayOfWeek>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), It.IsAny<int?>()))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<CaregiverSchedule>()))
            .ReturnsAsync((CaregiverSchedule s) => s);

        // Act
        var result = await _service.UpdateAsync(scheduleId, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DayOfWeek.Tuesday, result.DayOfWeek);
        Assert.Equal(new TimeOnly(10, 0), result.StartTime);
        Assert.Equal(new TimeOnly(18, 0), result.EndTime);
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task UpdateSchedule_SavesCorrectDataToDatabase()
    {
        // Arrange
        var scheduleId = 1;
        var existingSchedule = new CaregiverSchedule
        {
            Id = scheduleId,
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            IsActive = true
        };

        var updateRequest = new UpdateCaregiverScheduleRequest
        {
            StartTime = new TimeOnly(10, 0)
        };

        CaregiverSchedule? savedSchedule = null;

        _mockRepository.Setup(r => r.GetByIdAsync(scheduleId))
            .ReturnsAsync(existingSchedule);
        _mockRepository.Setup(r => r.HasOverlappingScheduleAsync(
                It.IsAny<int>(), It.IsAny<DayOfWeek>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), It.IsAny<int?>()))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<CaregiverSchedule>()))
            .Callback<CaregiverSchedule>(s => savedSchedule = s)
            .ReturnsAsync((CaregiverSchedule s) => s);

        // Act
        await _service.UpdateAsync(scheduleId, updateRequest);

        // Assert
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<CaregiverSchedule>()), Times.Once);
        Assert.NotNull(savedSchedule);
        Assert.Equal(new TimeOnly(10, 0), savedSchedule.StartTime);
    }

    [Fact]
    public async Task UpdateSchedule_PreservesUnchangedFields()
    {
        // Arrange
        var scheduleId = 1;
        var existingSchedule = new CaregiverSchedule
        {
            Id = scheduleId,
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            IsActive = true
        };

        var updateRequest = new UpdateCaregiverScheduleRequest
        {
            StartTime = new TimeOnly(10, 0)
            // Only updating StartTime, other fields should remain unchanged
        };

        _mockRepository.Setup(r => r.GetByIdAsync(scheduleId))
            .ReturnsAsync(existingSchedule);
        _mockRepository.Setup(r => r.HasOverlappingScheduleAsync(
                It.IsAny<int>(), It.IsAny<DayOfWeek>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), It.IsAny<int?>()))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<CaregiverSchedule>()))
            .ReturnsAsync((CaregiverSchedule s) => s);

        // Act
        var result = await _service.UpdateAsync(scheduleId, updateRequest);

        // Assert
        Assert.Equal(scheduleId, result.Id);
        Assert.Equal(1, result.CaregiverId); // Unchanged
        Assert.Equal(DayOfWeek.Monday, result.DayOfWeek); // Unchanged
        Assert.Equal(new TimeOnly(10, 0), result.StartTime); // Changed
        Assert.Equal(new TimeOnly(17, 0), result.EndTime); // Unchanged
        Assert.True(result.IsActive); // Unchanged
    }

    #endregion

    #region Edge Cases

    [Theory]
    [InlineData(true, false)]  // Active to Inactive
    [InlineData(false, true)]  // Inactive to Active
    public async Task UpdateSchedule_TogglesIsActive_Successfully(bool initialState, bool newState)
    {
        // Arrange
        var scheduleId = 1;
        var existingSchedule = new CaregiverSchedule
        {
            Id = scheduleId,
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            IsActive = initialState
        };

        var updateRequest = new UpdateCaregiverScheduleRequest
        {
            IsActive = newState
        };

        _mockRepository.Setup(r => r.GetByIdAsync(scheduleId))
            .ReturnsAsync(existingSchedule);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<CaregiverSchedule>()))
            .ReturnsAsync((CaregiverSchedule s) => s);

        // Act
        var result = await _service.UpdateAsync(scheduleId, updateRequest);

        // Assert
        Assert.Equal(newState, result.IsActive);
    }

    [Fact]
    public async Task UpdateSchedule_WithNoChanges_StillCallsUpdate()
    {
        // Arrange
        var scheduleId = 1;
        var existingSchedule = new CaregiverSchedule
        {
            Id = scheduleId,
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            IsActive = true
        };

        var updateRequest = new UpdateCaregiverScheduleRequest
        {
            // Empty request - no fields to update
        };

        _mockRepository.Setup(r => r.GetByIdAsync(scheduleId))
            .ReturnsAsync(existingSchedule);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<CaregiverSchedule>()))
            .ReturnsAsync((CaregiverSchedule s) => s);

        // Act
        var result = await _service.UpdateAsync(scheduleId, updateRequest);

        // Assert
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<CaregiverSchedule>()), Times.Once);
        Assert.Equal(existingSchedule.DayOfWeek, result.DayOfWeek);
        Assert.Equal(existingSchedule.StartTime, result.StartTime);
        Assert.Equal(existingSchedule.EndTime, result.EndTime);
        Assert.Equal(existingSchedule.IsActive, result.IsActive);
    }

    [Fact]
    public async Task UpdateSchedule_CallsOverlapCheckWithCorrectExcludedId()
    {
        // Arrange
        var scheduleId = 1;
        var existingSchedule = new CaregiverSchedule
        {
            Id = scheduleId,
            CaregiverId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            IsActive = true
        };

        var updateRequest = new UpdateCaregiverScheduleRequest
        {
            StartTime = new TimeOnly(10, 0)
        };

        _mockRepository.Setup(r => r.GetByIdAsync(scheduleId))
            .ReturnsAsync(existingSchedule);
        _mockRepository.Setup(r => r.HasOverlappingScheduleAsync(
                It.IsAny<int>(), It.IsAny<DayOfWeek>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), It.IsAny<int?>()))
            .ReturnsAsync(false);
        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<CaregiverSchedule>()))
            .ReturnsAsync((CaregiverSchedule s) => s);

        // Act
        await _service.UpdateAsync(scheduleId, updateRequest);

        // Assert
        _mockRepository.Verify(r => r.HasOverlappingScheduleAsync(
            1, // caregiverId
            DayOfWeek.Monday,
            new TimeOnly(10, 0), // new start time
            new TimeOnly(17, 0), // existing end time
            scheduleId), // excluded schedule ID
            Times.Once);
    }

    #endregion
}