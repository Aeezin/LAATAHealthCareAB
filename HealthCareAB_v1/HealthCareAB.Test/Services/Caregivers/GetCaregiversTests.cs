using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using HealthCareAB_v1.Services.Implementations;
using HealthCareAB_v1.Repositories.Interfaces;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Exceptions;
using HealthCareAB_v1.Models.DTOs.Caregiver;

namespace HealthCareAB.Test.Services.Caregivers
{
    public class GetCaregiversTests
    {
        private readonly Mock<ICaregiverRepository> _mockRepo;
        private readonly CaregiverService _service;

        public GetCaregiversTests()
        {
            _mockRepo = new Mock<ICaregiverRepository>();
            _service = new CaregiverService(_mockRepo.Object);
        }

        // Validation
        [Fact]
        public async Task GetAllCaregivers_ReturnsListOfCaregivers()
        {
            // Arrange
            var caregivers = new List<Caregiver>
            {
                new Caregiver { Id = 1, FirstName = "John", LastName = "Doe", Specialisation = "GP", Room = "101", UserId = 1 },
                new Caregiver { Id = 2, FirstName = "Jane", LastName = "Smith", Specialisation = "Surgeon", Room = "202", UserId = 2 }
            };
            _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(caregivers);

            // Act
            var result = await _service.GetAllCaregiversAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetCaregiverById_WithValidId_ReturnsCaregiver()
        {
            // Arrange
            var caregiver = new Caregiver { Id = 1, FirstName = "John", LastName = "Doe", Specialisation = "GP", Room = "101", UserId = 1 };
            _mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(caregiver);

            // Act
            var result = await _service.GetCaregiverByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("John", result.FirstName);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        public async Task GetCaregiverById_WithNegativeId_ThrowsCaregiverValidationException(int invalidId)
        {
            // Act & Assert
            await Assert.ThrowsAsync<CaregiverValidationException>(() => _service.GetCaregiverByIdAsync(invalidId));
        }

        [Fact]
        public async Task GetCaregiverById_WithInvalidId_ThrowsCaregiverValidationException() // Covers generic invalid positive if logic changed, but here mainly negative logic constraint
        {
             // Note: In current logic, <= 0 triggers validation.
             // This generic test name from requirements maps to the <= 0 check.
             // We can use InlineData for variety.
             await Assert.ThrowsAsync<CaregiverValidationException>(() => _service.GetCaregiverByIdAsync(0));
        }

        // Buisness logic
        [Fact]
        public async Task GetAllCaregivers_ReturnsCompleteDataWithAllFields()
        {
            // Arrange
            var caregiver = new Caregiver 
            { 
                Id = 1, 
                FirstName = "John", 
                LastName = "Doe", 
                Specialisation = "GP", 
                Room = "ROOM_NAME", 
                Bio = "My Bio", 
                Verified = true, 
                IsAcceptingPatients = true,
                UserId = 1
            };
            _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Caregiver> { caregiver });

            // Act
            var result = (await _service.GetAllCaregiversAsync()).First();

            // Assert
            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
            Assert.Equal("GP", result.Specialisation);
            Assert.Equal("ROOM_NAME", result.Room);
            Assert.Equal("My Bio", result.Bio);
            Assert.True(result.Verified);
            Assert.True(result.IsAcceptingPatients);
        }

        [Fact]
        public async Task GetCaregiverById_ReturnsCompleteCaregiverData()
        {
             // Arrange
            var caregiver = new Caregiver 
            { 
                Id = 10, 
                FirstName = "Alice", 
                LastName = "Wonder", 
                Specialisation = "Nobody", 
                Room = "000",
                UserId = 10
            };
            _mockRepo.Setup(repo => repo.GetByIdAsync(10)).ReturnsAsync(caregiver);

            // Act
            var result = await _service.GetCaregiverByIdAsync(10);

            // Assert
            Assert.Equal(10, result.Id);
            Assert.Equal("Alice", result.FirstName);
        }

        [Fact]
        public async Task GetCaregiverById_WhenCaregiverNotFound_ThrowsCaregiverNotFoundException()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((Caregiver?)null);

            // Act & Assert
            await Assert.ThrowsAsync<CaregiverNotFoundException>(() => _service.GetCaregiverByIdAsync(999));
        }

        [Fact]
        public async Task GetAllCaregivers_WhenDatabaseEmpty_ReturnsEmptyList()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Caregiver>());

            // Act
            var result = await _service.GetAllCaregiversAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        // Edge cases
        [Fact]
        public async Task GetAllCaregivers_WithLargeDataset_ReturnsAllRecords()
        {
            // Arrange
            var largeList = Enumerable.Range(1, 1000).Select(i => new Caregiver 
            { 
                Id = i, 
                FirstName = $"Name{i}", 
                LastName = "Last", 
                Specialisation = "Spec", 
                Room = "1",
                UserId = i
            }).ToList();
            
            _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(largeList);

            // Act
            var result = await _service.GetAllCaregiversAsync();

            // Assert
            Assert.Equal(1000, result.Count());
        }

        [Fact]
        public async Task GetCaregiverById_WithSpecialCharactersInData_ReturnsCorrectData()
        {
            // Arrange
            var caregiver = new Caregiver 
            { 
                Id = 5, 
                FirstName = "José", 
                LastName = "O'Connor", 
                Specialisation = "C# & .NET", 
                Room = "A-101", 
                UserId = 5 
            };
            _mockRepo.Setup(repo => repo.GetByIdAsync(5)).ReturnsAsync(caregiver);

            // Act
            var result = await _service.GetCaregiverByIdAsync(5);

            // Assert
            Assert.Equal("José", result.FirstName);
            Assert.Equal("O'Connor", result.LastName);
            Assert.Equal("C# & .NET", result.Specialisation);
        }

        [Fact]
        public async Task GetCaregiverById_WithZeroId_ThrowsCaregiverValidationException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<CaregiverValidationException>(() => _service.GetCaregiverByIdAsync(0));
        }

        [Fact]
        public async Task GetAllCaregivers_WhenDatabaseConnectionFails_ThrowsDatabaseException()
        {
             // Arrange
             // Simulating a database failure from the repository
             _mockRepo.Setup(repo => repo.GetAllAsync()).ThrowsAsync(new Exception("Database connection failed"));

             // Act & Assert
             var ex = await Assert.ThrowsAsync<Exception>(() => _service.GetAllCaregiversAsync());
             Assert.Equal("Database connection failed", ex.Message);
        }
    }
}
