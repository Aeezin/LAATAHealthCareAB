using HealthCareAB_v1.DTOs;
using HealthCareAB_v1.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace HealthCareAB_v1.Tests.Services;

public class AuthService_LoginPatient_Tests
{
    private readonly AuthServiceTestFixture _fixture;

    public AuthService_LoginPatient_Tests()
    {
        _fixture = new AuthServiceTestFixture();
    }

    #region Validation - Positive

    [Fact]
    public async Task LoginPatientAsync_WithValidPinAndPassword_ReturnsSuccess()
    {
        // Arrange
        var personalIdentityNumber = "199001011234";
        var password = "ValidPassword123!";
        var userId = 1;
        var expectedToken = "valid.jwt.token";

        var patient = new Patient
        {
            Id = 1,
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = "1990-01-01",
            PersonalIdentityNumber = personalIdentityNumber,
        };

        var user = new ApplicationUser { Id = userId, UserName = "johndoe" };

        _fixture
            .PatientRepositoryMock.Setup(x =>
                x.GetByPersonalIdentityNumberAsync(personalIdentityNumber)
            )
            .ReturnsAsync(patient);

        _fixture.UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

        _fixture
            .SignInManagerMock.Setup(x => x.PasswordSignInAsync(user, password, false, true))
            .ReturnsAsync(SignInResult.Success);

        _fixture.JwtTokenServiceMock.Setup(x => x.GenerateToken(user)).ReturnsAsync(expectedToken);

        _fixture
            .UserManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Patient" });

        // Act
        var (response, token) = await _fixture.AuthService.LoginPatientAsync(
            personalIdentityNumber,
            password
        );

        // Assert
        Assert.True(response.Success);
        Assert.Equal("Login successful", response.Message);
        Assert.Equal("johndoe", response.Username);
        Assert.Equal("John", response.FirstName);
        Assert.Equal("Doe", response.LastName);
        Assert.Contains("Patient", response.Roles);
        Assert.Equal(expectedToken, token);
        Assert.Equal(1, response.EntityId);
    }

    #endregion

    #region Validation - Negative

    [Theory]
    [InlineData(null, typeof(ArgumentNullException))]
    [InlineData("", typeof(ArgumentException))]
    [InlineData("   ", typeof(ArgumentException))]
    public async Task LoginPatientAsync_WithMissingPin_ThrowsArgumentException(
        string? invalidPin,
        Type expectedExceptionType
    )
    {
        // Arrange
        var password = "ValidPassword123!";

        // Act & Assert
        await Assert.ThrowsAsync(
            expectedExceptionType,
            () => _fixture.AuthService.LoginPatientAsync(invalidPin!, password)
        );
    }

    [Fact]
    public async Task LoginPatientAsync_WithInvalidPin_ReturnsFailure()
    {
        // Arrange
        var invalidPin = "000000000000";
        var password = "ValidPassword123!";

        _fixture
            .PatientRepositoryMock.Setup(x => x.GetByPersonalIdentityNumberAsync(invalidPin))
            .ReturnsAsync((Patient?)null);

        // Act
        var (response, token) = await _fixture.AuthService.LoginPatientAsync(invalidPin, password);

        // Assert
        Assert.False(response.Success);
        Assert.Equal("Invalid PIN or password", response.Message);
        Assert.Null(token);
    }

    [Theory]
    [InlineData(null, typeof(ArgumentNullException))]
    [InlineData("", typeof(ArgumentException))]
    [InlineData("   ", typeof(ArgumentException))]
    public async Task LoginPatientAsync_WithMissingPassword_ThrowsArgumentException(
        string? invalidPassword,
        Type expectedExceptionType
    )
    {
        // Arrange
        var personalIdentityNumber = "199001011234";

        // Act & Assert
        await Assert.ThrowsAsync(
            expectedExceptionType,
            () => _fixture.AuthService.LoginPatientAsync(personalIdentityNumber, invalidPassword!)
        );
    }

    [Fact]
    public async Task LoginPatientAsync_WithIncorrectCredentials_ReturnsFailure()
    {
        // Arrange
        var personalIdentityNumber = "199001011234";
        var wrongPassword = "WrongPassword123!";
        var userId = 1;

        var patient = new Patient
        {
            Id = 1,
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = "1990-01-01",
            PersonalIdentityNumber = personalIdentityNumber,
        };

        var user = new ApplicationUser { Id = userId, UserName = "johndoe" };

        _fixture
            .PatientRepositoryMock.Setup(x =>
                x.GetByPersonalIdentityNumberAsync(personalIdentityNumber)
            )
            .ReturnsAsync(patient);

        _fixture.UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

        _fixture
            .SignInManagerMock.Setup(x => x.PasswordSignInAsync(user, wrongPassword, false, true))
            .ReturnsAsync(SignInResult.Failed);

        // Act
        var (response, token) = await _fixture.AuthService.LoginPatientAsync(
            personalIdentityNumber,
            wrongPassword
        );

        // Assert
        Assert.False(response.Success);
        Assert.Equal("Invalid PIN or password", response.Message);
        Assert.Null(token);
    }

    #endregion

    #region Business Logic - Positive

    [Fact]
    public async Task LoginPatientAsync_OnSuccess_ReturnsValidJwtToken()
    {
        // Arrange
        var personalIdentityNumber = "199001011234";
        var password = "ValidPassword123!";
        var userId = 1;
        var expectedToken =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ";

        var patient = new Patient
        {
            Id = 1,
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = "1990-01-01",
            PersonalIdentityNumber = personalIdentityNumber,
        };

        var user = new ApplicationUser { Id = userId, UserName = "johndoe" };

        _fixture
            .PatientRepositoryMock.Setup(x =>
                x.GetByPersonalIdentityNumberAsync(personalIdentityNumber)
            )
            .ReturnsAsync(patient);

        _fixture.UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

        _fixture
            .SignInManagerMock.Setup(x => x.PasswordSignInAsync(user, password, false, true))
            .ReturnsAsync(SignInResult.Success);

        _fixture.JwtTokenServiceMock.Setup(x => x.GenerateToken(user)).ReturnsAsync(expectedToken);

        _fixture
            .UserManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Patient" });

        // Act
        var (response, token) = await _fixture.AuthService.LoginPatientAsync(
            personalIdentityNumber,
            password
        );

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
    public async Task LoginPatientAsync_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var personalIdentityNumber = "199001011234";
        var password = "ValidPassword123!";

        _fixture
            .PatientRepositoryMock.Setup(x =>
                x.GetByPersonalIdentityNumberAsync(personalIdentityNumber)
            )
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _fixture.AuthService.LoginPatientAsync(personalIdentityNumber, password)
        );

        Assert.Equal("Database connection failed", exception.Message);
    }

    [Fact]
    public async Task LoginPatientAsync_WhenUserManagerThrowsException_PropagatesException()
    {
        // Arrange
        var personalIdentityNumber = "199001011234";
        var password = "ValidPassword123!";
        var userId = 1;

        var patient = new Patient
        {
            Id = 1,
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = "1990-01-01",
            PersonalIdentityNumber = personalIdentityNumber,
        };

        _fixture
            .PatientRepositoryMock.Setup(x =>
                x.GetByPersonalIdentityNumberAsync(personalIdentityNumber)
            )
            .ReturnsAsync(patient);

        _fixture
            .UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ThrowsAsync(new Exception("Identity service unavailable"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _fixture.AuthService.LoginPatientAsync(personalIdentityNumber, password)
        );

        Assert.Equal("Identity service unavailable", exception.Message);
    }

    [Fact]
    public async Task LoginPatientAsync_WhenSignInManagerThrowsException_PropagatesException()
    {
        // Arrange
        var personalIdentityNumber = "199001011234";
        var password = "ValidPassword123!";
        var userId = 1;

        var patient = new Patient
        {
            Id = 1,
            UserId = userId,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = "1990-01-01",
            PersonalIdentityNumber = personalIdentityNumber,
        };

        var user = new ApplicationUser { Id = userId, UserName = "johndoe" };

        _fixture
            .PatientRepositoryMock.Setup(x =>
                x.GetByPersonalIdentityNumberAsync(personalIdentityNumber)
            )
            .ReturnsAsync(patient);

        _fixture.UserManagerMock.Setup(x => x.FindByIdAsync(userId.ToString())).ReturnsAsync(user);

        _fixture
            .SignInManagerMock.Setup(x => x.PasswordSignInAsync(user, password, false, true))
            .ThrowsAsync(new Exception("Authentication service unavailable"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _fixture.AuthService.LoginPatientAsync(personalIdentityNumber, password)
        );

        Assert.Equal("Authentication service unavailable", exception.Message);
    }

    #endregion
}
