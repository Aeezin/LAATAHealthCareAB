using HealthCareAB_v1.Configuration;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Repositories.Implementations;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services;
using HealthCareAB_v1.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace HealthCareAB_v1.Tests.Services;

public class AuthServiceTestFixture
{
    public Mock<IUserService> UserServiceMock { get; }
    public Mock<IJwtTokenService> JwtTokenServiceMock { get; }
    public Mock<UserManager<ApplicationUser>> UserManagerMock { get; }
    public Mock<SignInManager<ApplicationUser>> SignInManagerMock { get; }
    public Mock<IOptions<JwtSettings>> JwtSettingsMock { get; }
    public Mock<IWebHostEnvironment> WebHostEnvironmentMock { get; }
    public Mock<IHttpContextAccessor> HttpContextAccessorMock { get; }
    public Mock<IPatientRepository> PatientRepositoryMock { get; }
    public Mock<ICaregiverRepository> CaregiverRepositoryMock { get; }
    public AppDbContext DbContext { get; }
    public AuthService AuthService { get; }

    public AuthServiceTestFixture()
    {
        UserServiceMock = new Mock<IUserService>();
        JwtTokenServiceMock = new Mock<IJwtTokenService>();
        UserManagerMock = CreateUserManagerMock();
        SignInManagerMock = CreateSignInManagerMock(UserManagerMock);
        JwtSettingsMock = new Mock<IOptions<JwtSettings>>();
        WebHostEnvironmentMock = new Mock<IWebHostEnvironment>();
        HttpContextAccessorMock = new Mock<IHttpContextAccessor>();
        PatientRepositoryMock = new Mock<IPatientRepository>();
        CaregiverRepositoryMock = new Mock<ICaregiverRepository>();
        DbContext = CreateInMemoryDbContext();

        // Setup default JwtSettings
        JwtSettingsMock
            .Setup(x => x.Value)
            .Returns(
                new JwtSettings
                {
                    Secret = "test-secret-key-that-is-long-enough-for-testing-purposes",
                    Issuer = "test-issuer",
                    Audience = "test-audience",
                    ExpiryInMinutes = 60,
                }
            );

        // Setup default environment
        WebHostEnvironmentMock.Setup(x => x.EnvironmentName).Returns("Development");

        AuthService = new AuthService(
            UserServiceMock.Object,
            JwtTokenServiceMock.Object,
            UserManagerMock.Object,
            SignInManagerMock.Object,
            JwtSettingsMock.Object,
            WebHostEnvironmentMock.Object,
            HttpContextAccessorMock.Object,
            DbContext,
            PatientRepositoryMock.Object,
            CaregiverRepositoryMock.Object
        );
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        var optionsMock = new Mock<IOptions<IdentityOptions>>();
        var passwordHasherMock = new Mock<IPasswordHasher<ApplicationUser>>();
        var userValidators = new List<IUserValidator<ApplicationUser>>();
        var passwordValidators = new List<IPasswordValidator<ApplicationUser>>();
        var lookupNormalizerMock = new Mock<ILookupNormalizer>();
        var identityErrorDescriberMock = new Mock<IdentityErrorDescriber>();
        var serviceProviderMock = new Mock<IServiceProvider>();
        var loggerMock = new Mock<ILogger<UserManager<ApplicationUser>>>();

        optionsMock.Setup(o => o.Value).Returns(new IdentityOptions());

        return new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            optionsMock.Object,
            passwordHasherMock.Object,
            userValidators,
            passwordValidators,
            lookupNormalizerMock.Object,
            identityErrorDescriberMock.Object,
            serviceProviderMock.Object,
            loggerMock.Object
        );
    }

    private static Mock<SignInManager<ApplicationUser>> CreateSignInManagerMock(
        Mock<UserManager<ApplicationUser>> userManagerMock
    )
    {
        var contextAccessorMock = new Mock<IHttpContextAccessor>();
        var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        var optionsMock = new Mock<IOptions<IdentityOptions>>();
        var loggerMock = new Mock<ILogger<SignInManager<ApplicationUser>>>();
        var schemesMock = new Mock<IAuthenticationSchemeProvider>();
        var confirmationMock = new Mock<IUserConfirmation<ApplicationUser>>();

        optionsMock.Setup(o => o.Value).Returns(new IdentityOptions());

        return new Mock<SignInManager<ApplicationUser>>(
            userManagerMock.Object,
            contextAccessorMock.Object,
            userPrincipalFactoryMock.Object,
            optionsMock.Object,
            loggerMock.Object,
            schemesMock.Object,
            confirmationMock.Object
        );
    }

    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
