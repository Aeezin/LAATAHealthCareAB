using HealthCareAB_v1.DTOs;
using HealthCareAB_v1.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace HealthCareAB_v1.Tests.Services;

public class AuthService_LoginCaregiver_Tests
{
    private readonly AuthServiceTestFixture _fixture;

    public AuthService_LoginCaregiver_Tests()
    {
        _fixture = new AuthServiceTestFixture();
    }

    #region Validation - Positive

    [Fact]
    public async Task LoginCaregiverAsync_WithValidUsernameAndPassword_ReturnsSuccess()
    {
        // Arrange
        var username = "dr.smith";
        var password = "ValidPassword123!";
        var userId = 1;
        var expectedToken = "valid.jwt.token";

        var user = new ApplicationUser { Id = userId, UserName = username };

        var caregiver = new Caregiver
        {
            Id = 1,
            UserId = userId,
            FirstName = "Jane",
            LastName = "Smith",
            Specialisation = "Cardiology",
            Room = "A101",
        };

        _fixture.UserManagerMock.Setup(x => x.FindByNameAsync(username)).ReturnsAsync(user);

        _fixture
            .CaregiverRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(caregiver);

        _fixture
            .SignInManagerMock.Setup(x => x.PasswordSignInAsync(user, password, false, true))
            .ReturnsAsync(SignInResult.Success);

        _fixture.JwtTokenServiceMock.Setup(x => x.GenerateToken(user)).ReturnsAsync(expectedToken);

        _fixture
            .UserManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Caregiver" });

        // Act
        var (response, token) = await _fixture.AuthService.LoginCaregiverAsync(username, password);

        // Assert
        Assert.True(response.Success);
        Assert.Equal("Login successful", response.Message);
        Assert.Equal(username, response.Username);
        Assert.Equal("Jane", response.FirstName);
        Assert.Equal("Smith", response.LastName);
        Assert.Contains("Caregiver", response.Roles);
        Assert.Equal(expectedToken, token);
        Assert.Equal(1, response.EntityId);
    }

    #endregion

    #region Validation - Negative

    [Theory]
    [InlineData(null, typeof(ArgumentNullException))]
    [InlineData("", typeof(ArgumentException))]
    [InlineData("   ", typeof(ArgumentException))]
    public async Task LoginCaregiverAsync_WithMissingUsername_ThrowsArgumentException(
        string? invalidUsername,
        Type expectedExceptionType
    )
    {
        // Arrange
        var password = "ValidPassword123!";

        // Act & Assert
        await Assert.ThrowsAsync(
            expectedExceptionType,
            () => _fixture.AuthService.LoginCaregiverAsync(invalidUsername!, password)
        );
    }

    [Fact]
    public async Task LoginCaregiverAsync_WithInvalidUsername_ReturnsFailure()
    {
        // Arrange
        var invalidUsername = "nonexistent.user";
        var password = "ValidPassword123!";

        _fixture
            .UserManagerMock.Setup(x => x.FindByNameAsync(invalidUsername))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var (response, token) = await _fixture.AuthService.LoginCaregiverAsync(
            invalidUsername,
            password
        );

        // Assert
        Assert.False(response.Success);
        Assert.Equal("Invalid username or password", response.Message);
        Assert.Null(token);
    }

    [Theory]
    [InlineData(null, typeof(ArgumentNullException))]
    [InlineData("", typeof(ArgumentException))]
    [InlineData("   ", typeof(ArgumentException))]
    public async Task LoginCaregiverAsync_WithMissingPassword_ThrowsArgumentException(
        string? invalidPassword,
        Type expectedExceptionType
    )
    {
        // Arrange
        var username = "dr.smith";

        // Act & Assert
        await Assert.ThrowsAsync(
            expectedExceptionType,
            () => _fixture.AuthService.LoginCaregiverAsync(username, invalidPassword!)
        );
    }

    [Fact]
    public async Task LoginCaregiverAsync_WithIncorrectCredentials_ReturnsFailure()
    {
        // Arrange
        var username = "dr.smith";
        var wrongPassword = "WrongPassword123!";
        var userId = 1;

        var user = new ApplicationUser { Id = userId, UserName = username };

        var caregiver = new Caregiver
        {
            Id = 1,
            UserId = userId,
            FirstName = "Jane",
            LastName = "Smith",
            Specialisation = "Cardiology",
            Room = "A101",
        };

        _fixture.UserManagerMock.Setup(x => x.FindByNameAsync(username)).ReturnsAsync(user);

        _fixture
            .CaregiverRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(caregiver);

        _fixture
            .SignInManagerMock.Setup(x => x.PasswordSignInAsync(user, wrongPassword, false, true))
            .ReturnsAsync(SignInResult.Failed);

        // Act
        var (response, token) = await _fixture.AuthService.LoginCaregiverAsync(
            username,
            wrongPassword
        );

        // Assert
        Assert.False(response.Success);
        Assert.Equal("Invalid username or password", response.Message);
        Assert.Null(token);
    }

    [Fact]
    public async Task LoginCaregiverAsync_WhenUserExistsButNotCaregiver_ReturnsFailure()
    {
        // Arrange
        var username = "regularuser";
        var password = "ValidPassword123!";
        var userId = 1;

        var user = new ApplicationUser { Id = userId, UserName = username };

        _fixture.UserManagerMock.Setup(x => x.FindByNameAsync(username)).ReturnsAsync(user);

        _fixture
            .CaregiverRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync((Caregiver?)null);

        // Act
        var (response, token) = await _fixture.AuthService.LoginCaregiverAsync(username, password);

        // Assert
        Assert.False(response.Success);
        Assert.Equal("Invalid username or password", response.Message);
        Assert.Null(token);
    }

    #endregion

    #region Business Logic - Positive

    [Fact]
    public async Task LoginCaregiverAsync_OnSuccess_ReturnsValidJwtToken()
    {
        // Arrange
        var username = "dr.smith";
        var password = "ValidPassword123!";
        var userId = 1;
        var expectedToken =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkphbmUgU21pdGgiLCJpYXQiOjE1MTYyMzkwMjJ9";

        var user = new ApplicationUser { Id = userId, UserName = username };

        var caregiver = new Caregiver
        {
            Id = 1,
            UserId = userId,
            FirstName = "Jane",
            LastName = "Smith",
            Specialisation = "Cardiology",
            Room = "A101",
        };

        _fixture.UserManagerMock.Setup(x => x.FindByNameAsync(username)).ReturnsAsync(user);

        _fixture
            .CaregiverRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(caregiver);

        _fixture
            .SignInManagerMock.Setup(x => x.PasswordSignInAsync(user, password, false, true))
            .ReturnsAsync(SignInResult.Success);

        _fixture.JwtTokenServiceMock.Setup(x => x.GenerateToken(user)).ReturnsAsync(expectedToken);

        _fixture
            .UserManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Caregiver" });

        // Act
        var (response, token) = await _fixture.AuthService.LoginCaregiverAsync(username, password);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Equal(expectedToken, token);

        _fixture.JwtTokenServiceMock.Verify(x => x.GenerateToken(user), Times.Once);
    }

    #endregion

    #region Edge Cases - Negative

    [Fact]
    public async Task LoginCaregiverAsync_WhenUserManagerThrowsException_PropagatesException()
    {
        // Arrange
        var username = "dr.smith";
        var password = "ValidPassword123!";

        _fixture
            .UserManagerMock.Setup(x => x.FindByNameAsync(username))
            .ThrowsAsync(new Exception("Identity service unavailable"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _fixture.AuthService.LoginCaregiverAsync(username, password)
        );

        Assert.Equal("Identity service unavailable", exception.Message);
    }

    [Fact]
    public async Task LoginCaregiverAsync_WhenCaregiverRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var username = "dr.smith";
        var password = "ValidPassword123!";
        var userId = 1;

        var user = new ApplicationUser { Id = userId, UserName = username };

        _fixture.UserManagerMock.Setup(x => x.FindByNameAsync(username)).ReturnsAsync(user);

        _fixture
            .CaregiverRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _fixture.AuthService.LoginCaregiverAsync(username, password)
        );

        Assert.Equal("Database connection failed", exception.Message);
    }

    [Fact]
    public async Task LoginCaregiverAsync_WhenSignInManagerThrowsException_PropagatesException()
    {
        // Arrange
        var username = "dr.smith";
        var password = "ValidPassword123!";
        var userId = 1;

        var user = new ApplicationUser { Id = userId, UserName = username };

        var caregiver = new Caregiver
        {
            Id = 1,
            UserId = userId,
            FirstName = "Jane",
            LastName = "Smith",
            Specialisation = "Cardiology",
            Room = "A101",
        };

        _fixture.UserManagerMock.Setup(x => x.FindByNameAsync(username)).ReturnsAsync(user);

        _fixture
            .CaregiverRepositoryMock.Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(caregiver);

        _fixture
            .SignInManagerMock.Setup(x => x.PasswordSignInAsync(user, password, false, true))
            .ThrowsAsync(new Exception("Authentication service unavailable"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _fixture.AuthService.LoginCaregiverAsync(username, password)
        );

        Assert.Equal("Authentication service unavailable", exception.Message);
    }

    #endregion
}
