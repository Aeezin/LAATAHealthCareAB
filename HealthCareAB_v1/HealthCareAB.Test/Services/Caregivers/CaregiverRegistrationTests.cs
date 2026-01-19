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
    /// Unit tests for caregiver registration in <see cref="AuthService"/>.
    /// </summary>
    public class CaregiverRegistrationTests
    {
        /// <summary>
        /// Creates a valid <see cref="RegisterCaregiverDto"/> used in tests.
        /// </summary>
        private RegisterCaregiverDto ValidDto(string email = "caregiver@healthcare.se") =>
            new RegisterCaregiverDto
            {
                Email = email,
                Password = "ValidP@ssw0rd!",
                FirstName = "Anna",
                LastName = "Svensson",
                Specialisation = "Nurse",
                Room = "101",
                Bio = "Experienced nurse with 5 years experience",
            };

        /// <summary>
        /// Creates a mocked DB context and captures added caregivers.
        /// </summary>
        private (Mock<IAppDbContext> dbMock, List<Caregiver> added, FakeDbTransaction tx) MockDb()
        {
            var dbMock = new Mock<IAppDbContext>();
            var added = new List<Caregiver>();
            var tx = new FakeDbTransaction();

            // Intercept Caregivers.Add
            var caregiversDbSetMock = new Mock<DbSet<Caregiver>>();
            caregiversDbSetMock
                .Setup(s => s.Add(It.IsAny<Caregiver>()))
                .Callback<Caregiver>(c => added.Add(c))
                .Returns((EntityEntry<Caregiver>)null!);

            dbMock.SetupGet(d => d.Caregivers).Returns(caregiversDbSetMock.Object);
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
            IAppDbContext dbContext,
            Mock<ICaregiverRepository> caregiverRepositoryMock = null
        )
        {
            caregiverRepositoryMock ??= new Mock<ICaregiverRepository>();

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
                        u.Id = 1;
                        var hasher = new PasswordHasher<ApplicationUser>();
                        u.PasswordHash = hasher.HashPassword(u, pwd);
                    }
                );
            userMgr
                .Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Caregiver"))
                .ReturnsAsync(IdentityResult.Success);

            var caregiverRepoMock = new Mock<ICaregiverRepository>();
            caregiverRepoMock
                .Setup(r => r.SaveAsync(It.IsAny<Caregiver>()))
                .ReturnsAsync((Caregiver c) => c);

            var (dbMock, added, tx) = MockDb();
            var sut = CreateSut(userMgr, dbMock.Object, caregiverRepoMock);

            var dto = ValidDto();

            // Act
            var result = await sut.RegisterCaregiverAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("User registered successfully", result.Message);
            Assert.Equal(dto.Email, result.Username);
            Assert.Contains("Caregiver", result.Roles ?? new List<string>());
            Assert.True(tx.Committed);
            Assert.False(tx.RolledBack);
            caregiverRepoMock.Verify(r => r.SaveAsync(It.IsAny<Caregiver>()), Times.Once);
        }

        /// <summary>
        /// Verifies registration fails when email is already in use.
        /// </summary>
        [Fact]
        public async Task Register_Fails_When_Email_Already_In_Use()
        {
            // Arrange
            var userMgr = UserManagerMockHelper.Create();
            userMgr
                .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser { Email = "caregiver@healthcare.se" });

            var (dbMock, added, tx) = MockDb();
            var sut = CreateSut(userMgr, dbMock.Object);

            var dto = ValidDto();

            // Act
            var result = await sut.RegisterCaregiverAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Email is already taken", result.Message);
            // No transaction is started when email already exists, so no rollback occurs
            Assert.False(tx.Committed);
            userMgr.Verify(
                m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
                Times.Never
            );
        }

        /// <summary>
        /// Verifies password is hashed during registration.
        /// </summary>
        [Fact]
        public async Task Register_Hashes_Password_Correctly()
        {
            // Arrange
            var userMgr = UserManagerMockHelper.Create();
            var hashedPassword = string.Empty;

            userMgr
                .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);
            userMgr
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>(
                    (u, pwd) =>
                    {
                        u.Id = 2;
                        var hasher = new PasswordHasher<ApplicationUser>();
                        hashedPassword = hasher.HashPassword(u, pwd);
                        u.PasswordHash = hashedPassword;
                    }
                );
            userMgr
                .Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Caregiver"))
                .ReturnsAsync(IdentityResult.Success);

            var caregiverRepoMock = new Mock<ICaregiverRepository>();
            caregiverRepoMock
                .Setup(r => r.SaveAsync(It.IsAny<Caregiver>()))
                .ReturnsAsync((Caregiver c) => c);

            var (dbMock, added, tx) = MockDb();
            var sut = CreateSut(userMgr, dbMock.Object, caregiverRepoMock);

            var dto = ValidDto();

            // Act
            var result = await sut.RegisterCaregiverAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.NotEmpty(hashedPassword);
            Assert.NotEqual(dto.Password, hashedPassword); // Password should be hashed, not plaintext
            userMgr.Verify(
                m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password),
                Times.Once
            );
        }

        /// <summary>
        /// Verifies registration fails when password is too weak.
        /// </summary>
        [Fact]
        public async Task Register_Fails_With_Weak_Password()
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

            var dto = ValidDto("weak.password@healthcare.se");
            dto.Password = "weak"; // Too weak

            // Act
            var result = await sut.RegisterCaregiverAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Password too weak", result.Message);
            Assert.True(tx.RolledBack);
        }

        /// <summary>
        /// Verifies caregiver role is assigned correctly.
        /// </summary>
        [Fact]
        public async Task Register_Assigns_Caregiver_Role()
        {
            // Arrange
            var userMgr = UserManagerMockHelper.Create();
            var assignedRole = string.Empty;

            userMgr
                .Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);
            userMgr
                .Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>(
                    (u, pwd) =>
                    {
                        u.Id = 3;
                    }
                );
            userMgr
                .Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>(
                    (u, role) =>
                    {
                        assignedRole = role;
                    }
                );

            var caregiverRepoMock = new Mock<ICaregiverRepository>();
            caregiverRepoMock
                .Setup(r => r.SaveAsync(It.IsAny<Caregiver>()))
                .ReturnsAsync((Caregiver c) => c);

            var (dbMock, added, tx) = MockDb();
            var sut = CreateSut(userMgr, dbMock.Object, caregiverRepoMock);

            var dto = ValidDto();

            // Act
            var result = await sut.RegisterCaregiverAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Caregiver", assignedRole);
            Assert.Contains("Caregiver", result.Roles);
        }

        /// <summary>
        /// Verifies caregiver entity is created with correct data.
        /// </summary>
        [Fact]
        public async Task Register_Creates_Caregiver_With_Correct_Data()
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
                        u.Id = 4;
                    }
                );
            userMgr
                .Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Caregiver"))
                .ReturnsAsync(IdentityResult.Success);

            var capturedCaregiver = (Caregiver)null;
            var caregiverRepoMock = new Mock<ICaregiverRepository>();
            caregiverRepoMock
                .Setup(r => r.SaveAsync(It.IsAny<Caregiver>()))
                .ReturnsAsync(
                    (Caregiver c) =>
                    {
                        capturedCaregiver = c;
                        return c;
                    }
                );

            var (dbMock, added, tx) = MockDb();
            var sut = CreateSut(userMgr, dbMock.Object, caregiverRepoMock);

            var dto = ValidDto();

            // Act
            var result = await sut.RegisterCaregiverAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(capturedCaregiver);
            Assert.Equal(dto.FirstName, capturedCaregiver.FirstName);
            Assert.Equal(dto.LastName, capturedCaregiver.LastName);
            Assert.Equal(dto.Specialisation, capturedCaregiver.Specialisation);
            Assert.Equal(dto.Room, capturedCaregiver.Room);
            Assert.Equal(dto.Bio, capturedCaregiver.Bio);
            Assert.True(capturedCaregiver.IsAcceptingPatients);
            Assert.False(capturedCaregiver.Verified);
        }

        /// <summary>
        /// Verifies transaction is rolled back when role assignment fails.
        /// </summary>
        [Fact]
        public async Task Register_Rolls_Back_When_Role_Assignment_Fails()
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
                        u.Id = 5;
                    }
                );
            userMgr
                .Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Caregiver"))
                .ReturnsAsync(
                    IdentityResult.Failed(
                        new IdentityError { Description = "Role assignment failed" }
                    )
                );
            userMgr
                .Setup(m => m.DeleteAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            var (dbMock, added, tx) = MockDb();
            var sut = CreateSut(userMgr, dbMock.Object);

            var dto = ValidDto();

            // Act
            var result = await sut.RegisterCaregiverAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Role assignment failed", result.Message);
            Assert.True(tx.RolledBack);
            Assert.False(tx.Committed);
            userMgr.Verify(m => m.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Once);
        }
    }
}
