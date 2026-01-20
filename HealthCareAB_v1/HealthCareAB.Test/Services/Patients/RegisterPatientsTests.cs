using HealthCareAB_v1.Configuration;
using HealthCareAB_v1.DTOs;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services;
using HealthCareAB_v1.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace HealthCareAB.Test.Services
{
    /// <summary>
    /// Unit tests for patient registration in <see cref="AuthService"/>.
    /// </summary>
    public class RegisterPatientsTests
    {
        /// <summary>
        /// Creates a valid <see cref="RegisterDto"/> used in tests.
        /// </summary>
        private RegisterDto ValidDto(string email = "example@email.com") =>
            new RegisterDto
            {
                Email = email,
                Password = "ValidP@ssw0rd!",
                FirstName = "Wa",
                LastName = "Lee",
                PhoneNumber = "0700000000",
                PersonalIdentityNumber = "19900101-1234",
            };

        /// <summary>
        /// Creates a mocked DB context and captures added patients.
        /// /// </summary>
        private (Mock<IAppDbContext> dbMock, List<Patient> added, FakeDbTransaction tx) MockDb()
        {
            var dbMock = new Mock<IAppDbContext>();
            var added = new List<Patient>();
            var tx = new FakeDbTransaction();

            // Intercept Patients.Add
            var patientsDbSetMock = new Mock<DbSet<Patient>>();
            patientsDbSetMock
                .Setup(s => s.Add(It.IsAny<Patient>()))
                .Callback<Patient>(p => added.Add(p))
                .Returns((EntityEntry<Patient>)null!);

            dbMock.SetupGet(d => d.Patients).Returns(patientsDbSetMock.Object);
            dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            dbMock
                .Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(tx);

            return (dbMock, added, tx);
        }

        /// <summary>
        /// Creates the system under test with mocked dependencies.
        /// </summary>
        private AuthService CreateSut(
            Mock<UserManager<ApplicationUser>> userManagerMock,
            IAppDbContext dbContext
        )
        {
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                Options.Create(new IdentityOptions()),
                Mock.Of<ILogger<SignInManager<ApplicationUser>>>(),
                Mock.Of<IAuthenticationSchemeProvider>(),
                Mock.Of<IUserConfirmation<ApplicationUser>>()
            );

            IOptions<JwtSettings> jwtOptions = Options.Create(new JwtSettings());

            var patientRepositoryMock = new Mock<IPatientRepository>();
            var caregiverRepositoryMock = new Mock<ICaregiverRepository>();

            var authService = new AuthService(
                Mock.Of<IUserService>(),
                Mock.Of<IJwtTokenService>(),
                userManagerMock.Object,
                signInManagerMock.Object,
                jwtOptions,
                Mock.Of<IWebHostEnvironment>(),
                Mock.Of<IHttpContextAccessor>(),
                dbContext,
                patientRepositoryMock.Object,
                caregiverRepositoryMock.Object
            );

            return authService;
        }

        /// <summary>
        /// Verifies registration succeeds with valid input.
        /// </summary>
        [Fact]
        public async Task Register_Succeeds_With_Valid_Input()
        {
            // Arrange
            var userMgr = UserManagerMockHelper.Create();
            userMgr
                .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);
            userMgr
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>(
                    (u, pwd) =>
                    {
                        u.Id = 123;
                        var hasher = new PasswordHasher<ApplicationUser>();
                        u.PasswordHash = hasher.HashPassword(u, pwd);
                    }
                );
            userMgr
                .Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Patient"))
                .ReturnsAsync(IdentityResult.Success);

            var (dbMock, added, tx) = MockDb();
            var sut = CreateSut(userMgr, dbMock.Object);

            var dto = ValidDto();

            // Act
            var result = await sut.RegisterPatientAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("User registered successfully", result.Message);
            Assert.Equal(dto.Email, result.Username);
            Assert.Contains("Patient", result.Roles ?? new List<string>());
            Assert.Single(added);
            Assert.True(tx.Committed);
            Assert.False(tx.RolledBack);
            dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies registration fails when email is invalid.
        /// </summary>
        [Fact]
        public async Task Register_Fails_When_Email_Invalid()
        {
            // Arrange
            var userMgr = UserManagerMockHelper.Create();
            userMgr
                .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);
            userMgr
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(
                    IdentityResult.Failed(new IdentityError { Description = "Invalid email" })
                );

            var (dbMock, added, tx) = MockDb();
            var sut = CreateSut(userMgr, dbMock.Object);

            var dto = ValidDto(email: "bad-email");

            // Act
            var result = await sut.RegisterPatientAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Invalid email", result.Message);
            Assert.Empty(added);
            Assert.True(tx.RolledBack);
            Assert.False(tx.Committed);
            dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Verifies registration fails when email is already taken.
        /// </summary>
        [Fact]
        public async Task Register_Fails_When_Email_Already_In_Use()
        {
            // Arrange
            var userMgr = UserManagerMockHelper.Create();
            userMgr
                .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser { Email = "example@email.com" });

            var (dbMock, added, tx) = MockDb();
            var sut = CreateSut(userMgr, dbMock.Object);

            var dto = ValidDto();

            // Act
            var result = await sut.RegisterPatientAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Email is already taken", result.Message);
            Assert.Empty(added);

            dbMock.Verify(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            userMgr.Verify(
                m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
                Times.Never
            );
        }

        /// <summary>
        /// Verifies patient is persisted on successful registration.
        /// </summary>
        [Fact]
        public async Task Register_Persists_Patient_On_Success()
        {
            // Arrange
            var userMgr = UserManagerMockHelper.Create();
            userMgr
                .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);
            userMgr
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>((u, _) => u.Id = 123);
            userMgr
                .Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Patient"))
                .ReturnsAsync(IdentityResult.Success);

            var (dbMock, added, tx) = MockDb();
            var sut = CreateSut(userMgr, dbMock.Object);

            var dto = ValidDto();

            // Act
            var result = await sut.RegisterPatientAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Single(added);
            Assert.Equal(dto.FirstName, added[0].FirstName);
            Assert.Equal(123, added[0].UserId);
            dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies password is stored hashed (not in plain text).
        /// </summary>
        [Fact]
        public async Task Register_Stores_Password_Hashed_And_Salted()
        {
            // Arrange
            var userMgr = UserManagerMockHelper.Create();
            ApplicationUser? createdUser = null;

            userMgr
                .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);

            userMgr
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>(
                    (u, pwd) =>
                    {
                        createdUser = u;
                        u.Id = 123;
                        var hasher = new PasswordHasher<ApplicationUser>();
                        u.PasswordHash = hasher.HashPassword(u, pwd);
                    }
                );

            userMgr
                .Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Patient"))
                .ReturnsAsync(IdentityResult.Success);

            var (dbMock, added, tx) = MockDb();
            var sut = CreateSut(userMgr, dbMock.Object);

            var dto = ValidDto();

            // Act
            var result = await sut.RegisterPatientAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(createdUser);
            Assert.False(string.IsNullOrWhiteSpace(createdUser!.PasswordHash));
            Assert.NotEqual(dto.Password, createdUser.PasswordHash);

            var hasherCheck = new PasswordHasher<ApplicationUser>();
            var verification = hasherCheck.VerifyHashedPassword(
                createdUser!,
                createdUser!.PasswordHash!,
                dto.Password
            );
            Assert.NotEqual(PasswordVerificationResult.Failed, verification);
        }

        /// <summary>
        /// Verifies patient is not created when registration fails.
        /// </summary>
        [Fact]
        public async Task Register_Does_Not_Create_Patient_When_Validation_Fails()
        {
            // Arrange
            var userMgr = UserManagerMockHelper.Create();
            userMgr
                .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);
            userMgr
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(
                    IdentityResult.Failed(new IdentityError { Description = "Password too weak" })
                );

            var (dbMock, added, tx) = MockDb();
            var sut = CreateSut(userMgr, dbMock.Object);

            var dto = ValidDto();

            // Act
            var result = await sut.RegisterPatientAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Password too weak", result.Message);
            Assert.Empty(added);
            Assert.True(tx.RolledBack);
            Assert.False(tx.Committed);
            dbMock.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
