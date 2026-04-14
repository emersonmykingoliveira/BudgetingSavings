using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Interfaces;
using BudgetingSavings.API.Models.Enums;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Models.Responses;
using BudgetingSavings.API.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace BudgetingSavings.Tests.UnitTests
{
    public class RewardServiceUnitTests : IDisposable
    {
        private readonly ApiDbContext _db;
        private readonly IAccountService _accountsService;
        private readonly IValidator<CreateRewardRequest> _validator;
        private readonly IConfiguration _config;
        private readonly IRewardService _service;

        public RewardServiceUnitTests()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _db = new ApiDbContext(options);
            _accountsService = Substitute.For<IAccountService>();
            _validator = Substitute.For<IValidator<CreateRewardRequest>>();
            
            var myConfiguration = new Dictionary<string, string>
            {
                {"RewardSettings:PointsFactor", "0.01"},
                {"RewardSettings:CashBackFactor", "0.05"}
            };

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration!)
                .Build();

            _service = new RewardService(_db, _accountsService, _validator, _config);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        [Fact]
        public async Task GetAllRewardsAsync_ShouldReturnRewards_WhenCustomerExists()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer { Id = customerId, Name = "Test" };
            var reward = new Reward { Id = Guid.NewGuid(), CustomerId = customerId, Points = 100 };
            
            await _db.Customers.AddAsync(customer);
            await _db.Rewards.AddAsync(reward);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetAllRewardsAsync(customerId, CancellationToken.None);

            // Assert
            Assert.Single(result);
            Assert.Equal(reward.Id, result[0].Id);
        }

        [Fact]
        public async Task GetAllRewardsAsync_ShouldThrow_WhenCustomerDoesNotExist()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.GetAllRewardsAsync(Guid.NewGuid(), CancellationToken.None));
        }

        [Fact]
        public async Task GetRewardByIdAsync_ShouldReturnReward_WhenExistsAndNotRedeemed()
        {
            // Arrange
            var reward = new Reward { Id = Guid.NewGuid(), Points = 100, Redeemed = false };
            await _db.Rewards.AddAsync(reward);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetRewardByIdAsync(reward.Id, CancellationToken.None);

            // Assert
            Assert.Equal(reward.Id, result.Id);
        }

        [Fact]
        public async Task GetRewardByIdAsync_ShouldThrow_WhenRedeemed()
        {
            // Arrange
            var reward = new Reward { Id = Guid.NewGuid(), Redeemed = true };
            await _db.Rewards.AddAsync(reward);
            await _db.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.GetRewardByIdAsync(reward.Id, CancellationToken.None));
        }

        [Fact]
        public async Task RedeemRewardAsync_ShouldProcessSuccessfully_WhenValid()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer { Id = customerId, Name = "Test", DateOfBirth = DateTime.UtcNow.AddYears(-20) };
            var account = new Account { Id = Guid.NewGuid(), CustomerId = customerId, Balance = 1000, AccountNumber = "12345", Currency = CurrencyType.USD, CreatedDate = DateTime.UtcNow };
            var reward = new Reward { Id = Guid.NewGuid(), CustomerId = customerId, Points = 500, Redeemed = false };

            await _db.Customers.AddAsync(customer);
            await _db.Accounts.AddAsync(account);
            await _db.Rewards.AddAsync(reward);
            await _db.SaveChangesAsync();

            var request = new RedeemRewardRequest { Id = reward.Id, CustomerId = customerId };

            // Act
            var result = await _service.RedeemRewardAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5m, result.CashBack); // 500 * 0.01
            Assert.True(reward.Redeemed);
            await _accountsService.Received(1).UpdateAccountBalanceAsync(account.Id, 5m, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task RewardHandlerAsync_ShouldCreateNewRewardWithWelcomeBonus_WhenFirstTransaction()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer { Id = customerId, Name = "Test", DateOfBirth = DateTime.UtcNow.AddYears(-20) };
            await _db.Customers.AddAsync(customer);
            await _db.SaveChangesAsync();

            var request = new CreateRewardRequest
            {
                CustomerId = customerId,
                Amount = 100,
                TransactionType = TransactionType.Credit,
                TransactionCategory = TransactionCategory.Savings
            };

            _validator.ValidateAsync(request, Arg.Any<CancellationToken>())
                .Returns(new ValidationResult());

            // Act
            await _service.RewardHandlerAsync(request, CancellationToken.None);

            // Assert
            var newReward = await _db.Rewards.FirstOrDefaultAsync(r => r.CustomerId == customerId);
            Assert.NotNull(newReward);
            Assert.Equal(1100, newReward.Points); // (100 * 10) + 100 welcome bonus
        }

        [Fact]
        public async Task RewardHandlerAsync_ShouldAddBonusPoints_OnFirstSavingOfMonth()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            await _db.Customers.AddAsync(new Customer { Id = customerId, Name = "Test", DateOfBirth = DateTime.UtcNow.AddYears(-20) });
            await _db.Accounts.AddAsync(new Account { Id = accountId, CustomerId = customerId, AccountNumber = "12345", Currency = CurrencyType.USD, CreatedDate = DateTime.UtcNow });
            
            var existingReward = new Reward { Id = Guid.NewGuid(), CustomerId = customerId, Points = 200, Redeemed = false };
            await _db.Rewards.AddAsync(existingReward);

            await _db.Transactions.AddAsync(new Transaction 
            { 
                Id = Guid.NewGuid(),
                AccountId = accountId, 
                TransactionDateTime = DateTime.UtcNow, 
                TransactionCategory = TransactionCategory.Savings,
                Amount = 10,
                TransactionType = TransactionType.Credit,
                Currency = CurrencyType.USD
            });
            await _db.SaveChangesAsync();

            var request = new CreateRewardRequest
            {
                CustomerId = customerId,
                Amount = 10,
                TransactionType = TransactionType.Credit,
                TransactionCategory = TransactionCategory.Savings
            };

            _validator.ValidateAsync(request, Arg.Any<CancellationToken>())
                .Returns(new ValidationResult());

            // Act
            await _service.RewardHandlerAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal(350, existingReward.Points); // 200 + (10 * 10) + 50 bonus
        }

        [Fact]
        public async Task RewardHandlerAsync_ShouldDeductPoints_OnDebitTransaction()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            await _db.Customers.AddAsync(new Customer { Id = customerId, Name = "Test", DateOfBirth = DateTime.UtcNow.AddYears(-20) });
            var reward = new Reward { Id = Guid.NewGuid(), CustomerId = customerId, Points = 500, Redeemed = false };
            await _db.Rewards.AddAsync(reward);
            await _db.SaveChangesAsync();

            var request = new CreateRewardRequest
            {
                CustomerId = customerId,
                Amount = 20,
                TransactionType = TransactionType.Debit,
                TransactionCategory = TransactionCategory.Savings
            };

            _validator.ValidateAsync(request, Arg.Any<CancellationToken>())
                .Returns(new ValidationResult());

            // Act
            await _service.RewardHandlerAsync(request, CancellationToken.None);

            // Assert
            Assert.Equal(300, reward.Points); // 500 - (20 * 10)
        }
    }
}
