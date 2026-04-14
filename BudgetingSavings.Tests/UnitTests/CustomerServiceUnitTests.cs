using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Interfaces;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace BudgetingSavings.Tests.UnitTests
{
    public class CustomerServiceUnitTests : IDisposable
    {
        private readonly ApiDbContext _db;
        private readonly IValidator<CreateCustomerRequest> _createValidator;
        private readonly IValidator<UpdateCustomerRequest> _updateValidator;
        private readonly ICustomerService _service;

        public CustomerServiceUnitTests()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new ApiDbContext(options);
            _createValidator = Substitute.For<IValidator<CreateCustomerRequest>>();
            _updateValidator = Substitute.For<IValidator<UpdateCustomerRequest>>();
            _service = new CustomerService(_db, _createValidator, _updateValidator);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        [Fact]
        public async Task CreateCustomerAsync_ShouldReturnCustomer_WhenValid()
        {
            // Arrange
            var request = new CreateCustomerRequest
            {
                Name = "John Doe",
                Email = "john@example.com",
                PhoneNumber = "1234567890",
                DateOfBirth = DateTime.UtcNow.AddYears(-20)
            };

            // Act
            var result = await _service.CreateCustomerAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(request.Email, result.Email);
            Assert.NotEqual(Guid.Empty, result.Id);
        }

        [Fact]
        public async Task CreateCustomerAsync_ShouldThrow_WhenEmailAlreadyExists()
        {
            // Arrange
            var email = "duplicate@example.com";
            await _db.Customers.AddAsync(new Customer { Id = Guid.NewGuid(), Email = email, Name = "Existing" });
            await _db.SaveChangesAsync();

            var request = new CreateCustomerRequest
            {
                Name = "New User",
                Email = email,
                PhoneNumber = "0987654321",
                DateOfBirth = DateTime.UtcNow.AddYears(-20)
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateCustomerAsync(request, CancellationToken.None));
            Assert.Equal("A customer with this email already exists.", exception.Message);
        }

        [Fact]
        public async Task CreateCustomerAsync_ShouldThrow_WhenPhoneAlreadyExists()
        {
            // Arrange
            var phone = "1234567890";
            await _db.Customers.AddAsync(new Customer { Id = Guid.NewGuid(), PhoneNumber = phone, Name = "Existing", Email = "other@example.com" });
            await _db.SaveChangesAsync();

            var request = new CreateCustomerRequest
            {
                Name = "New User",
                Email = "new@example.com",
                PhoneNumber = phone,
                DateOfBirth = DateTime.UtcNow.AddYears(-20)
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateCustomerAsync(request, CancellationToken.None));
            Assert.Equal("A customer with this phone number already exists.", exception.Message);
        }

        [Fact]
        public async Task CreateCustomerAsync_ShouldThrow_WhenUnder18()
        {
            // Arrange
            var request = new CreateCustomerRequest
            {
                Name = "Young User",
                Email = "young@example.com",
                PhoneNumber = "111222333",
                DateOfBirth = DateTime.UtcNow.AddYears(-17) // Under 18
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateCustomerAsync(request, CancellationToken.None));
            Assert.Equal("Customer must be at least 18 years old.", exception.Message);
        }

        [Fact]
        public async Task GetAllCustomersAsync_ShouldReturnList()
        {
            // Arrange
            await _db.Customers.AddAsync(new Customer { Id = Guid.NewGuid(), Name = "C1", Email = "c1@e.com" });
            await _db.Customers.AddAsync(new Customer { Id = Guid.NewGuid(), Name = "C2", Email = "c2@e.com" });
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetAllCustomersAsync(CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetCustomerByIdAsync_ShouldReturnCustomer_WhenExists()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer { Id = customerId, Name = "John", Email = "j@e.com" };
            await _db.Customers.AddAsync(customer);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetCustomerByIdAsync(customerId, CancellationToken.None);

            // Assert
            Assert.Equal(customerId, result.Id);
            Assert.Equal("John", result.Name);
        }

        [Fact]
        public async Task UpdateCustomerAsync_ShouldUpdate_WhenValid()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer { Id = customerId, Name = "Old Name", Email = "old@e.com", DateOfBirth = DateTime.UtcNow.AddYears(-25) };
            await _db.Customers.AddAsync(customer);
            await _db.SaveChangesAsync();

            var request = new UpdateCustomerRequest
            {
                Id = customerId,
                Name = "New Name",
                Email = "new@e.com",
                DateOfBirth = DateTime.UtcNow.AddYears(-25),
                PhoneNumber = "999888777"
            };

            // Act
            var result = await _service.UpdateCustomerAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal("New Name", result.Name);
            Assert.Equal("new@e.com", result.Email);
            
            var updated = await _db.Customers.FindAsync(customerId);
            Assert.Equal("New Name", updated?.Name);
        }

        [Fact]
        public async Task UpdateCustomerAsync_ShouldThrow_WhenEmailExistsOnOtherCustomer()
        {
            // Arrange
            var c1Id = Guid.NewGuid();
            var c2Id = Guid.NewGuid();
            var sharedEmail = "duplicate@e.com";
            await _db.Customers.AddAsync(new Customer { Id = c1Id, Email = sharedEmail, Name = "C1" });
            await _db.Customers.AddAsync(new Customer { Id = c2Id, Email = "c2@e.com", Name = "C2", DateOfBirth = DateTime.UtcNow.AddYears(-25) });
            await _db.SaveChangesAsync();

            var request = new UpdateCustomerRequest
            {
                Id = c2Id,
                Email = sharedEmail,
                Name = "C2 Updated",
                DateOfBirth = DateTime.UtcNow.AddYears(-25)
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.UpdateCustomerAsync(request, CancellationToken.None));
            Assert.Equal("A customer with this email already exists.", exception.Message);
        }

        [Fact]
        public async Task DeleteCustomerAsync_ShouldRemove_WhenNoAccounts()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            await _db.Customers.AddAsync(new Customer { Id = customerId, Name = "Delete Me", Email = "d@e.com" });
            await _db.SaveChangesAsync();

            // Act
            await _service.DeleteCustomerAsync(customerId, CancellationToken.None);

            // Assert
            var deleted = await _db.Customers.FindAsync(customerId);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteCustomerAsync_ShouldThrow_WhenHasAccounts()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            await _db.Customers.AddAsync(new Customer { Id = customerId, Name = "Cannot Delete", Email = "c@e.com" });
            await _db.Accounts.AddAsync(new Account { Id = Guid.NewGuid(), CustomerId = customerId, AccountNumber = "1234" });
            await _db.SaveChangesAsync();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.DeleteCustomerAsync(customerId, CancellationToken.None));
            Assert.Equal("Cannot delete a customer with existing accounts.", exception.Message);
        }
    }
}
