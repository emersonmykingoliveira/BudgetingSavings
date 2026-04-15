using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Services;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Models.Enums;

namespace BudgetingSavings.Tests.UnitTests
{
    public class BudgetServiceUnitTests : IDisposable
    {
        private readonly ApiDbContext _db;
        private readonly IValidator<CreateBudgetRequest> _createValidator;
        private readonly IValidator<UpdateBudgetRequest> _updateValidator;
        private readonly IBudgetService _service;

        public BudgetServiceUnitTests()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new ApiDbContext(options);
            _createValidator = Substitute.For<IValidator<CreateBudgetRequest>>();
            _updateValidator = Substitute.For<IValidator<UpdateBudgetRequest>>();
            _service = new BudgetService(_db, _createValidator, _updateValidator);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        [Fact]
        public async Task CreateBudgetAsync_ShouldReturnBudget_WhenValid()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            await _db.Customers.AddAsync(new Customer { Id = customerId, Name = "Test" });
            await _db.SaveChangesAsync();

            var request = new CreateBudgetRequest
            {
                CustomerId = customerId,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddMonths(1),
                LimitAmount = 1000m,
                Currency = CurrencyType.USD
            };

            // Act
            var result = await _service.CreateBudgetAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(customerId, result.Value.CustomerId);
            Assert.Equal(1000m, result.Value.LimitAmount);
            Assert.Equal(CurrencyType.USD, result.Value.Currency);
        }

        [Fact]
        public async Task CreateBudgetAsync_ShouldReturnFailure_WhenCustomerDoesNotExist()
        {
            // Arrange
            var request = new CreateBudgetRequest { CustomerId = Guid.NewGuid() };

            // Act
            var result = await _service.CreateBudgetAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Customer does not exist.", result.Error);
        }

        [Fact]
        public async Task GetBudgetByIdAsync_ShouldReturnBudget_WhenExists()
        {
            // Arrange
            var budgetId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            await _db.Budgets.AddAsync(new Budget
            {
                Id = budgetId,
                CustomerId = customerId,
                LimitAmount = 500m,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddDays(10)
            });
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetBudgetByIdAsync(budgetId, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(budgetId, result.Value.Id);
            Assert.Equal(500m, result.Value.LimitAmount);
        }

        [Fact]
        public async Task GetBudgetByIdAsync_ShouldReturnFailure_WhenNotExists()
        {
            // Act
            var result = await _service.GetBudgetByIdAsync(Guid.NewGuid(), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Budget does not exist.", result.Error);
        }

        [Fact]
        public async Task GetAllBudgetsAsync_ShouldReturnList_WhenCustomerExists()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            await _db.Customers.AddAsync(new Customer { Id = customerId, Name = "Test" });
            await _db.Budgets.AddAsync(new Budget { Id = Guid.NewGuid(), CustomerId = customerId, LimitAmount = 100m });
            await _db.Budgets.AddAsync(new Budget { Id = Guid.NewGuid(), CustomerId = customerId, LimitAmount = 200m });
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetAllBudgetsAsync(customerId, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task UpdateBudgetAsync_ShouldUpdate_WhenValid()
        {
            // Arrange
            var budgetId = Guid.NewGuid();
            var budget = new Budget { Id = budgetId, LimitAmount = 100m };
            await _db.Budgets.AddAsync(budget);
            await _db.SaveChangesAsync();

            var request = new UpdateBudgetRequest
            {
                Id = budgetId,
                LimitAmount = 200m,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddMonths(2),
                Currency = CurrencyType.EUR
            };

            // Act
            var result = await _service.UpdateBudgetAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(200m, result.Value.LimitAmount);
            Assert.Equal(CurrencyType.EUR, result.Value.Currency);
            
            var updated = await _db.Budgets.FindAsync(budgetId);
            Assert.Equal(200m, updated?.LimitAmount);
        }

        [Fact]
        public async Task DeleteBudgetAsync_ShouldRemove_WhenExists()
        {
            // Arrange
            var budgetId = Guid.NewGuid();
            await _db.Budgets.AddAsync(new Budget { Id = budgetId });
            await _db.SaveChangesAsync();

            // Act
            await _service.DeleteBudgetAsync(budgetId, CancellationToken.None);

            // Assert
            var deleted = await _db.Budgets.FindAsync(budgetId);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task GetBudgetStatusAsync_ShouldReturnCorrectStatus_WhenWithinLimit()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var budgetId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddDays(-1);
            var endTime = DateTime.UtcNow.AddDays(1);

            await _db.Customers.AddAsync(new Customer { Id = customerId, Name = "Test" });
            await _db.Accounts.AddAsync(new Account { Id = accountId, CustomerId = customerId, AccountNumber = "1" });
            await _db.Budgets.AddAsync(new Budget
            {
                Id = budgetId,
                CustomerId = customerId,
                StartTime = startTime,
                EndTime = endTime,
                LimitAmount = 1000m
            });

            // Transactions within budget period and of type Debit
            await _db.Transactions.AddAsync(new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                CustomerId = customerId,
                Amount = -100m,
                TransactionType = TransactionType.Debit,
                TransactionDateTime = DateTime.UtcNow
            });
            await _db.Transactions.AddAsync(new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                CustomerId = customerId,
                Amount = -200m,
                TransactionType = TransactionType.Debit,
                TransactionDateTime = DateTime.UtcNow
            });

            // Transaction out of period
            await _db.Transactions.AddAsync(new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = 500m,
                TransactionType = TransactionType.Debit,
                TransactionDateTime = DateTime.UtcNow.AddDays(-5)
            });

            // Transaction of different type (Credit)
            await _db.Transactions.AddAsync(new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = 1000m,
                TransactionType = TransactionType.Credit,
                TransactionDateTime = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetBudgetStatusAsync(budgetId, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(300m, result.Value.SpentAmount); // Only 100 + 200
            Assert.Equal(700m, result.Value.RemainingAmount);
            Assert.False(result.Value.IsExceeded);
        }

        [Fact]
        public async Task GetBudgetStatusAsync_ShouldReturnExceeded_WhenOverLimit()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var budgetId = Guid.NewGuid();
            var startTime = DateTime.Parse("2025-01-01");
            var endTime = DateTime.Parse("2025-01-31");

            await _db.Customers.AddAsync(new Customer { Id = customerId, Name = "Test" });
            await _db.Accounts.AddAsync(new Account { Id = accountId, CustomerId = customerId, AccountNumber = "1" });
            await _db.Budgets.AddAsync(new Budget
            {
                Id = budgetId,
                CustomerId = customerId,
                StartTime = startTime,
                EndTime = endTime,
                LimitAmount = 100m
            });

            await _db.Transactions.AddAsync(new Transaction
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                CustomerId = customerId,
                Amount = -150m,
                TransactionType = TransactionType.Debit,
                TransactionDateTime = DateTime.Parse("2025-01-15")
            });
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetBudgetStatusAsync(budgetId, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.True(result.Value.IsExceeded);
            Assert.Equal(150m, result.Value.SpentAmount);
            Assert.Equal(-50m, result.Value.RemainingAmount);
        }
    }
}
