using HealthCareAB_v1.Configuration;
using HealthCareAB_v1.DTOs;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services;
using HealthCareAB_v1.Services.Interfaces;
using HealthCareAB.Test.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;
using Moq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace HealthCareAB.Test.Services
{
    public class AuthServiceRegisterTests
    {
        // Your DTO uses string DateOfBirth
        private RegisterDto ValidDto(string email = "tony@example.com") => new RegisterDto
        {
            Email = email,
            Password = "ValidP@ssw0rd!",
            ConfirmPassword = "ValidP@ssw0rd!",
            FirstName = "Tony",
            Lastname = "Gullstrand",
            PhoneNumber = "0700000000",
            DateOfBirth = "1990-01-01", // string
            PersonalIdentityNumber = "19900101-1234"
        };

        private (Mock<IAppDbContext> dbMock, List<Patient> addedPatients, FakeDbTransaction tx) MockDb()
        {
            var dbMock = new Mock<IAppDbContext>();
            var added = new List<Patient>();
            var tx = new FakeDbTransaction();

            var patientsDbSetMock = new Mock<DbSet<Patient>>();
            patientsDbSetMock.Setup(s => s.Add(It.IsAny<Patient>()))
                .Returns<Patient>(p =>
                {
                    added.Add(p);
                    return Mock.Of<EntityEntry<Patient>>();
                });

            dbMock.SetupGet(d => d.Patients).Returns(patientsDbSetMock.Object);
            dbMock.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            dbMock.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tx);

            return (dbMock, added, tx);
        }

        private AuthService CreateSut(Mock<UserManager<ApplicationUser>> userMgr, IAppDbContext dbContext)
        {
            // Construct a valid SignInManager mock to avoid the "no parameterless constructor" error
            var signInMgr = SignInManagerMockHelper.Create(userMgr);

            // Minimal JwtSettings (not used in RegisterAsync)
            var jwtOptions = Options.Create(new JwtSettings());

            return new AuthService(
                userService: Mock.Of<IUserService>(),              // not used in RegisterAsync
                jwtTokenService: Mock.Of<IJwtTokenService>(),      // not used in RegisterAsync
                userManager: userMgr.Object,
                signInManager: signInMgr.Object,
                jwtSettings: jwtOptions,
                environment: Mock.Of<IWebHostEnvironment>(),
                httpContextAccessor: Mock.Of<IHttpContextAccessor>(),
                dbContext: dbContext                                // IAppDbContext injected
            );
        }

        // 1) Positive: success with valid input
        [Fact]
        public async Task Register_Succeeds_With_Valid_Input()
        {
            var userMgr = UserManagerMockHelper.Create();
            userMgr.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
            userMgr.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                   .ReturnsAsync(IdentityResult.Success)
                   .Callback<ApplicationUser, string>((u, pwd) =>
                   {
                       // IMPORTANT: ApplicationUser uses int Id
                       u.Id = 123;
                       // Optional hashing simulation (not strictly needed for this test)
                       var hasher = new PasswordHasher<ApplicationUser>();
                       u.PasswordHash = hasher.HashPassword(u, pwd);
                   });
            userMgr.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Patient"))
                   .ReturnsAsync(IdentityResult.Success);

            var (db, added, tx) = MockDb();
            var sut = CreateSut(userMgr, db.Object);

            var dto = ValidDto();
            var result = await sut.RegisterAsync(dto);

            Assert.True(result.Success);
            Assert.Equal("User registered successfully", result.Message);
            Assert.Equal(dto.Email, result.Username);
            Assert.Contains("Patient", result.Roles ?? new List<string>());
            Assert.Single(added);
            Assert.True(tx.Committed);
            Assert.False(tx.RolledBack);
            db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        // 2) Negative: invalid email
        [Fact]
        public async Task Register_Fails_When_Email_Invalid()
        {
            var userMgr = UserManagerMockHelper.Create();
            userMgr.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
            userMgr.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                   .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid email" }));

            var (db, added, tx) = MockDb();
            var sut = CreateSut(userMgr, db.Object);

            var dto = ValidDto(email: "bad-email");
            var result = await sut.RegisterAsync(dto);

            Assert.False(result.Success);
            Assert.Contains("Invalid email", result.Message);
            Assert.Empty(added);
            Assert.True(tx.RolledBack);
            Assert.False(tx.Committed);
            db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        // 3) Negative: passwords mismatch
        [Fact]
        public async Task Register_Fails_When_Passwords_Do_Not_Match()
        {
            var userMgr = UserManagerMockHelper.Create();
            userMgr.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
            userMgr.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                   .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Passwords do not match" }));

            var (db, added, tx) = MockDb();
            var sut = CreateSut(userMgr, db.Object);

            var dto = ValidDto();
            dto.ConfirmPassword = "DifferentPassword!";
            var result = await sut.RegisterAsync(dto);

            Assert.False(result.Success);
            Assert.Contains("Passwords do not match", result.Message);
            Assert.Empty(added);
            Assert.True(tx.RolledBack);
            Assert.False(tx.Committed);
        }

        // 4) Negative: email already in use
        [Fact]
        public async Task Register_Fails_When_Email_Already_In_Use()
        {
            var userMgr = UserManagerMockHelper.Create();
            userMgr.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                   .ReturnsAsync(new ApplicationUser { Email = "tony@example.com" });

            var (db, added, _) = MockDb();
            var sut = CreateSut(userMgr, db.Object);

            var dto = ValidDto();
            var result = await sut.RegisterAsync(dto);

            Assert.False(result.Success);
            Assert.Equal("Email is already taken", result.Message);
            Assert.Empty(added);

            // Transaction should not start on this early exit path
            db.Verify(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
            db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            userMgr.Verify(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        // 5) Business: patient persisted intent
        [Fact]
        public async Task Register_Persists_Patient_On_Success()
        {
            var userMgr = UserManagerMockHelper.Create();
            userMgr.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
            userMgr.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                   .ReturnsAsync(IdentityResult.Success)
                   .Callback<ApplicationUser, string>((u, _) => u.Id = 123);
            userMgr.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Patient"))
                   .ReturnsAsync(IdentityResult.Success);

            var (db, added, _) = MockDb();
            var sut = CreateSut(userMgr, db.Object);

            var dto = ValidDto();
            var result = await sut.RegisterAsync(dto);

            Assert.True(result.Success);
            Assert.Single(added);
            Assert.Equal(dto.FirstName, added[0].FirstName);
            Assert.Equal(123, added[0].UserId);
            db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        // 6) Business: password hashed and salted
        [Fact]
        public async Task Register_Stores_Password_Hashed_And_Salted()
        {
            var userMgr = UserManagerMockHelper.Create();
            ApplicationUser? createdUser = null;

            userMgr.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                   .ReturnsAsync((ApplicationUser?)null);

            userMgr.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                   .ReturnsAsync(IdentityResult.Success)
                   .Callback<ApplicationUser, string>((u, pwd) =>
                   {
                       createdUser = u;
                       u.Id = 123;
                       var hasher = new PasswordHasher<ApplicationUser>();
                       u.PasswordHash = hasher.HashPassword(u, pwd);
                   });

            userMgr.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Patient"))
                   .ReturnsAsync(IdentityResult.Success);

            var (db, _, _) = MockDb();
            var sut = CreateSut(userMgr, db.Object);

            var dto = ValidDto();
            var result = await sut.RegisterAsync(dto);

            Assert.True(result.Success);
            Assert.NotNull(createdUser);
            Assert.False(string.IsNullOrWhiteSpace(createdUser!.PasswordHash));
            Assert.NotEqual(dto.Password, createdUser.PasswordHash);

            var hasherCheck = new PasswordHasher<ApplicationUser>();
            var verification = hasherCheck.VerifyHashedPassword(createdUser!, createdUser!.PasswordHash!, dto.Password);
            Assert.NotEqual(PasswordVerificationResult.Failed, verification);
        }

        // 7) Business: no patient created when validation fails
        [Fact]
        public async Task Register_Does_Not_Create_Patient_When_Validation_Fails()
        {
            var userMgr = UserManagerMockHelper.Create();
            userMgr.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
            userMgr.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                   .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

            var (db, added, tx) = MockDb();
            var sut = CreateSut(userMgr, db.Object);

            var dto = ValidDto();
            var result = await sut.RegisterAsync(dto);

            Assert.False(result.Success);
            Assert.Contains("Password too weak", result.Message);
            Assert.Empty(added);
            Assert.True(tx.RolledBack);
            Assert.False(tx.Committed);
            db.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
