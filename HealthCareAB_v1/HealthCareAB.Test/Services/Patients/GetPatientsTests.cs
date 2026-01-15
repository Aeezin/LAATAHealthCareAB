using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthCareAB_v1.Exceptions;
using HealthCareAB_v1.Models.DTOs.Patient;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Services.Implementations;
using Moq;
using Xunit;

namespace HealthCareAB.Test.Services.Patients
{
    public class GetPatientsTests
    {
        private readonly Mock<IPatientRepository> _mockRepo;
        private readonly PatientService _service;

        public GetPatientsTests()
        {
            _mockRepo = new Mock<IPatientRepository>();
            _service = new PatientService(_mockRepo.Object);
        }

        // Validation

        [Fact]
        public async Task GetAllPatients_ReturnsListOfPatients()
        {
            // Arrange
            var patients = new List<Patient>
            {
                new Patient
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    UserId = 1,
                    DateOfBirth = "1980-01-01",
                    PersonalIdentityNumber = "19800101-1234",
                    PhoneNumber = "1234567890",
                },
                new Patient
                {
                    Id = 2,
                    FirstName = "Jane",
                    LastName = "Smith",
                    UserId = 2,
                    DateOfBirth = "1990-02-02",
                    PersonalIdentityNumber = "19900202-5678",
                    PhoneNumber = "0987654321",
                },
            };
            _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(patients);

            // Act
            var result = await _service.GetAllPatientsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetPatientById_WithValidId_ReturnsPatient()
        {
            // Arrange
            var patient = new Patient
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                UserId = 1,
                DateOfBirth = "1980-01-01",
                PersonalIdentityNumber = "19800101-1234",
            };
            _mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(patient);

            // Act
            var result = await _service.GetPatientByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("John", result.FirstName);
        }

        [Fact]
        public async Task GetPatientById_WithInvalidId_ThrowsPatientValidationException()
        {
            // Act & Assert
            // Assuming "Invalid Id" here refers to 0 or negative per the negative checks in other tests,
            // but explicitly requested as a separate test case.
            // The service checks if (id <= 0).
            await Assert.ThrowsAsync<PatientValidationException>(
                () => _service.GetPatientByIdAsync(0)
            );
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task GetPatientById_WithNegativeId_ThrowsPatientValidationException(
            int invalidId
        )
        {
            // Act & Assert
            await Assert.ThrowsAsync<PatientValidationException>(
                () => _service.GetPatientByIdAsync(invalidId)
            );
        }

        // Business Logic

        [Fact]
        public async Task GetAllPatients_ReturnsCompleteDataWithAllFields()
        {
            // Arrange
            var patient = new Patient
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                UserId = 1,
                DateOfBirth = "1980-01-01",
                PersonalIdentityNumber = "19800101-1234",
                PhoneNumber = "1234567890",
            };
            _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Patient> { patient });

            // Act
            var result = (await _service.GetAllPatientsAsync()).First();

            // Assert
            Assert.Equal(1, result.Id);
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
            Assert.Equal("1980-01-01", result.DateOfBirth);
            Assert.Equal("19800101-1234", result.PersonalIdentityNumber);
            Assert.Equal("1234567890", result.PhoneNumber);
        }

        [Fact]
        public async Task GetPatientById_ReturnsCompletePatientData()
        {
            // Arrange
            var patient = new Patient
            {
                Id = 10,
                FirstName = "Alice",
                LastName = "Wonder",
                UserId = 10,
                DateOfBirth = "2000-05-05",
                PersonalIdentityNumber = "20000505-9999",
                PhoneNumber = "555-5555",
            };
            _mockRepo.Setup(repo => repo.GetByIdAsync(10)).ReturnsAsync(patient);

            // Act
            var result = await _service.GetPatientByIdAsync(10);

            // Assert
            Assert.Equal(10, result.Id);
            Assert.Equal("Alice", result.FirstName);
            Assert.Equal("Wonder", result.LastName);
            Assert.Equal("555-5555", result.PhoneNumber);
        }

        [Fact]
        public async Task GetPatientById_WhenPatientNotFound_ThrowsPatientNotFoundException()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((Patient?)null);

            // Act & Assert
            await Assert.ThrowsAsync<PatientNotFoundException>(
                () => _service.GetPatientByIdAsync(999)
            );
        }

        [Fact]
        public async Task GetAllPatients_WhenDatabaseEmpty_ReturnsEmptyList()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Patient>());

            // Act
            var result = await _service.GetAllPatientsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // Edge Cases

        [Fact]
        public async Task GetAllPatients_WithLargeDataset_ReturnsAllRecords()
        {
            // Arrange
            var largeList = Enumerable
                .Range(1, 1000)
                .Select(i => new Patient
                {
                    Id = i,
                    FirstName = $"Name{i}",
                    LastName = "Last",
                    UserId = i,
                    DateOfBirth = "1990-01-01",
                    PersonalIdentityNumber = $"19900101-{i:0000}",
                })
                .ToList();

            _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(largeList);

            // Act
            var result = await _service.GetAllPatientsAsync();

            // Assert
            Assert.Equal(1000, result.Count());
        }

        [Fact]
        public async Task GetPatientById_WithSpecialCharactersInData_ReturnsCorrectData()
        {
            // Arrange
            var patient = new Patient
            {
                Id = 5,
                FirstName = "José",
                LastName = "O'Connor",
                UserId = 5,
                DateOfBirth = "1985-12-12",
                PersonalIdentityNumber = "19851212-0000",
            };
            _mockRepo.Setup(repo => repo.GetByIdAsync(5)).ReturnsAsync(patient);

            // Act
            var result = await _service.GetPatientByIdAsync(5);

            // Assert
            Assert.Equal("José", result.FirstName);
            Assert.Equal("O'Connor", result.LastName);
        }

        [Fact]
        public async Task GetPatientById_WithZeroId_ThrowsPatientValidationException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<PatientValidationException>(
                () => _service.GetPatientByIdAsync(0)
            );
        }

        [Fact]
        public async Task GetAllPatients_WhenDatabaseConnectionFails_ThrowsDatabaseException()
        {
            // Arrange
            _mockRepo
                .Setup(repo => repo.GetAllAsync())
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.GetAllPatientsAsync());
            Assert.Equal("Database connection failed", ex.Message);
        }
    }
}
