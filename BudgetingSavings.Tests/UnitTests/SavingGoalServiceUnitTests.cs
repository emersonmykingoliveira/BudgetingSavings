using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Models.Enums;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;
using BudgetingSavings.API.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NSubstitute;
using Xunit;

namespace BudgetingSavings.Tests.UnitTests
{
    public class SavingGoalServiceUnitTests : IDisposable
    {
        private readonly ApiDbContext _db;
        private readonly IValidator<CreateSavingGoalRequest> _createValidator;
        private readonly IValidator<UpdateSavingGoalRequest> _updateValidator;
        private readonly ISavingGoalService _service;

        public SavingGoalServiceUnitTests()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _db = new ApiDbContext(options);
            _createValidator = Substitute.For<IValidator<CreateSavingGoalRequest>>();
            _updateValidator = Substitute.For<IValidator<UpdateSavingGoalRequest>>();
            
            _service = new SavingGoalService(_db, _createValidator, _updateValidator);

            _createValidator.ValidateAsync(Arg.Any<CreateSavingGoalRequest>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new ValidationResult()));
            _updateValidator.ValidateAsync(Arg.Any<UpdateSavingGoalRequest>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new ValidationResult()));
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        [Fact]
        public async Task CreateSavingGoalAsync_ShouldReturnGoal_WhenValidRequest()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            await _db.Customers.AddAsync(new Customer { Id = customerId, Name = "Test", DateOfBirth = DateTime.UtcNow.AddYears(-20) });
            await _db.SaveChangesAsync();

            var request = new CreateSavingGoalRequest
            {
                CustomerId = customerId,
                Name = "New Car",
                TargetAmount = 10000,
                TargetDate = DateTime.UtcNow.AddYears(1)
            };

            // Act
            var result = await _service.CreateSavingGoalAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(request.TargetAmount, result.TargetAmount);
            var goalInDb = await _db.SavingGoals.FindAsync(result.Id);
            Assert.NotNull(goalInDb);
        }

        [Fact]
        public async Task CreateSavingGoalAsync_ShouldThrow_WhenCustomerDoesNotExist()
        {
            // Arrange
            var request = new CreateSavingGoalRequest { CustomerId = Guid.NewGuid() };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateSavingGoalAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task CreateSavingGoalAsync_ShouldThrow_WhenActiveGoalsLimitReached()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            await _db.Customers.AddAsync(new Customer { Id = customerId, Name = "Test", DateOfBirth = DateTime.UtcNow.AddYears(-20) });
            
            for (int i = 0; i < 5; i++)
            {
                await _db.SavingGoals.AddAsync(new SavingGoal 
                { 
                    Id = Guid.NewGuid(), 
                    CustomerId = customerId, 
                    Name = $"Goal {i}", 
                    TargetDate = DateTime.UtcNow.AddYears(1) 
                });
            }
            await _db.SaveChangesAsync();

            var request = new CreateSavingGoalRequest { CustomerId = customerId, TargetDate = DateTime.UtcNow.AddYears(1) };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateSavingGoalAsync(request, CancellationToken.None));
            Assert.Equal("Customer cannot have more than 5 active saving goals.", exception.Message);
        }

        [Fact]
        public async Task GetSavingGoalStatusAsync_ShouldReturnCorrectStatus_InProgress()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var goalId = Guid.NewGuid();
            
            await _db.Customers.AddAsync(new Customer { Id = customerId, Name = "Test", DateOfBirth = DateTime.UtcNow.AddYears(-20) });
            await _db.Accounts.AddAsync(new Account { Id = accountId, CustomerId = customerId, AccountNumber = "123", Currency = CurrencyType.USD });
            
            var goal = new SavingGoal 
            { 
                Id = goalId, 
                CustomerId = customerId, 
                Name = "Travel", 
                TargetAmount = 1000, 
                StartDate = DateTime.UtcNow.AddDays(-10),
                TargetDate = DateTime.UtcNow.AddDays(20) 
            };
            await _db.SavingGoals.AddAsync(goal);

            await _db.Transactions.AddAsync(new Transaction 
            { 
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = 200,
                TransactionType = TransactionType.Credit,
                TransactionCategory = TransactionCategory.Savings,
                TransactionDateTime = DateTime.UtcNow.AddDays(-5),
                Currency = CurrencyType.USD
            });
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetSavingGoalStatusAsync(goalId, CancellationToken.None);

            // Assert
            Assert.Equal(SavingGoalStatus.InProgress, result.Status);
            Assert.Equal(200, result.SavedAmount);
            Assert.Equal(20, result.ProgressPercentage);
            Assert.Equal(800, result.RemainingAmount);
        }

        [Fact]
        public async Task UpdateSavingGoalAsync_ShouldThrow_WhenTargetLowerThanSaved()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var goalId = Guid.NewGuid();

            await _db.Customers.AddAsync(new Customer { Id = customerId, Name = "Test", DateOfBirth = DateTime.UtcNow.AddYears(-20) });
            await _db.Accounts.AddAsync(new Account { Id = accountId, CustomerId = customerId, AccountNumber = "123", Currency = CurrencyType.USD });

            var goal = new SavingGoal 
            { 
                Id = goalId, 
                CustomerId = customerId, 
                Name = "Old Goal", 
                TargetAmount = 1000,
                StartDate = DateTime.UtcNow.AddDays(-10),
                TargetDate = DateTime.UtcNow.AddDays(20)
            };
            await _db.SavingGoals.AddAsync(goal);

            await _db.Transactions.AddAsync(new Transaction 
            { 
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Amount = 500,
                TransactionType = TransactionType.Credit,
                TransactionCategory = TransactionCategory.Savings,
                TransactionDateTime = DateTime.UtcNow.AddDays(-5),
                Currency = CurrencyType.USD
            });
            await _db.SaveChangesAsync();

            var request = new UpdateSavingGoalRequest
            {
                Id = goalId,
                Name = "Updated Goal",
                TargetAmount = 400, // Lower than 500 saved
                TargetDate = DateTime.UtcNow.AddDays(20)
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.UpdateSavingGoalAsync(request, CancellationToken.None));
            Assert.Equal("Target amount cannot be lower than the amount already saved.", exception.Message);
        }

        [Fact]
        public async Task GetSavingSuggestions_ShouldCalculateCorrectly()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            await _db.Customers.AddAsync(new Customer { Id = customerId, Name = "Test", DateOfBirth = DateTime.UtcNow.AddYears(-20) });
            
            await _db.Transactions.AddRangeAsync(
                new Transaction { Id = Guid.NewGuid(), CustomerId = customerId, Amount = 5000, TransactionType = TransactionType.Credit, Currency = CurrencyType.USD },
                new Transaction { Id = Guid.NewGuid(), CustomerId = customerId, Amount = 2000, TransactionType = TransactionType.Debit, Currency = CurrencyType.USD },
                new Transaction { Id = Guid.NewGuid(), CustomerId = customerId, Amount = 1000, TransactionType = TransactionType.Debit, Currency = CurrencyType.USD }
            );
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetSavingSuggestions(customerId, CancellationToken.None);

            // Assert
            Assert.Equal(5000, result.Income);
            Assert.Equal(3000, result.Expenses);
            Assert.Equal(2000, result.Disposable);
        }

        [Fact]
        public async Task GetAllSavingGoalsAsync_ShouldReturnList_WhenCustomerExists()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            await _db.Customers.AddAsync(new Customer { Id = customerId, Name = "Test", DateOfBirth = DateTime.UtcNow.AddYears(-20) });
            await _db.SavingGoals.AddRangeAsync(
                new SavingGoal { Id = Guid.NewGuid(), CustomerId = customerId, Name = "Goal 1" },
                new SavingGoal { Id = Guid.NewGuid(), CustomerId = customerId, Name = "Goal 2" }
            );
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetAllSavingGoalsAsync(customerId, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, g => g.Name == "Goal 1");
            Assert.Contains(result, g => g.Name == "Goal 2");
        }

        [Fact]
        public async Task GetSavingGoalByIdAsync_ShouldReturnGoal_WhenExists()
        {
            // Arrange
            var goalId = Guid.NewGuid();
            var goal = new SavingGoal { Id = goalId, Name = "Travel", CustomerId = Guid.NewGuid() };
            await _db.SavingGoals.AddAsync(goal);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetSavingGoalByIdAsync(goalId, CancellationToken.None);

            // Assert
            Assert.Equal(goalId, result.Id);
            Assert.Equal("Travel", result.Name);
        }

        [Fact]
        public async Task DeleteSavingGoalAsync_ShouldRemoveGoal_WhenExists()
        {
            // Arrange
            var goalId = Guid.NewGuid();
            var goal = new SavingGoal { Id = goalId, Name = "Delete Me", CustomerId = Guid.NewGuid() };
            await _db.SavingGoals.AddAsync(goal);
            await _db.SaveChangesAsync();

            // Act
            await _service.DeleteSavingGoalAsync(goalId, CancellationToken.None);

            // Assert
            var goalInDb = await _db.SavingGoals.FindAsync(goalId);
            Assert.Null(goalInDb);
        }

        [Fact]
        public async Task UpdateSavingGoalAsync_ShouldUpdateFields_WhenValid()
        {
            // Arrange
            var goalId = Guid.NewGuid();
            var goal = new SavingGoal { Id = goalId, Name = "Old Name", TargetAmount = 1000, StartDate = DateTime.UtcNow.AddDays(-1), TargetDate = DateTime.UtcNow.AddDays(10), CustomerId = Guid.NewGuid() };
            await _db.SavingGoals.AddAsync(goal);
            await _db.SaveChangesAsync();

            var request = new UpdateSavingGoalRequest
            {
                Id = goalId,
                Name = "Updated Name",
                TargetAmount = 2000,
                TargetDate = DateTime.UtcNow.AddDays(20)
            };

            // Act
            var result = await _service.UpdateSavingGoalAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal("Updated Name", result.Name);
            Assert.Equal(2000, result.TargetAmount);
            var goalInDb = await _db.SavingGoals.FindAsync(goalId);
            Assert.Equal("Updated Name", goalInDb!.Name);
        }
    }
}
