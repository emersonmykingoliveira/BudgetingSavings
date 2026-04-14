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
using NSubstitute;
using Xunit;

namespace BudgetingSavings.Tests.UnitTests
{
    public class TransactionServiceUnitTests : IDisposable
    {
        private readonly ApiDbContext _db;
        private readonly IValidator<CreateTransactionRequest> _createValidator;
        private readonly ITransactionService _service;
        private readonly IRewardService _rewardService;
        public TransactionServiceUnitTests()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _db = new ApiDbContext(options);

            _createValidator = Substitute.For<IValidator<CreateTransactionRequest>>();
            _rewardService = Substitute.For<IRewardService>();
            _service = new TransactionService(_db, _rewardService, _createValidator);

            _createValidator.ValidateAsync(Arg.Any<CreateTransactionRequest>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new ValidationResult()));
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        [Fact]
        public async Task CreateTransactionAsync_ShouldReturnTransaction_WhenValidRequest()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            await _db.Customers.AddAsync(new Customer { Id = customerId, Name = "Test", DateOfBirth = DateTime.UtcNow.AddYears(-20) });
            await _db.Accounts.AddAsync(new Account { Id = accountId, CustomerId = customerId, AccountNumber = "123", Currency = CurrencyType.USD, Balance = 1000 });
            await _db.SaveChangesAsync();

            var request = new CreateTransactionRequest
            {
                CustomerId = customerId,
                AccountId = accountId,
                Amount = 100,
                Currency = CurrencyType.USD,
                TransactionType = TransactionType.Debit,
                TransactionCategory = TransactionCategory.General
            };

            // Act
            var result = await _service.CreateTransactionAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.Amount * -1, result.Amount);
            var account = await _db.Accounts.FindAsync(accountId);
            Assert.Equal(900, account?.Balance);
            await _rewardService.Received(1).RewardHandlerAsync(Arg.Any<CreateRewardRequest>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task CreateTransactionAsync_ShouldThrow_WhenCustomerDoesNotExist()
        {
            // Arrange
            var request = new CreateTransactionRequest { CustomerId = Guid.NewGuid() };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateTransactionAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task CreateTransactionAsync_ShouldThrow_WhenAccountDoesNotExist()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            await _db.Customers.AddAsync(new Customer { Id = customerId, Name = "Test", DateOfBirth = DateTime.UtcNow.AddYears(-20) });
            await _db.SaveChangesAsync();

            var request = new CreateTransactionRequest { CustomerId = customerId, AccountId = Guid.NewGuid() };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateTransactionAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task CreateTransactionAsync_ShouldThrow_WhenAccountDoesNotBelongToCustomer()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var otherCustomerId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            await _db.Customers.AddRangeAsync(
                new Customer { Id = customerId, Name = "Test", DateOfBirth = DateTime.UtcNow.AddYears(-20) },
                new Customer { Id = otherCustomerId, Name = "Other", DateOfBirth = DateTime.UtcNow.AddYears(-20) }
            );
            await _db.Accounts.AddAsync(new Account { Id = accountId, CustomerId = otherCustomerId, AccountNumber = "123", Currency = CurrencyType.USD });
            await _db.SaveChangesAsync();

            var request = new CreateTransactionRequest { CustomerId = customerId, AccountId = accountId };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateTransactionAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task CreateTransactionAsync_ShouldThrow_WhenCurrencyMismatch()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            await _db.Customers.AddAsync(new Customer { Id = customerId, Name = "Test", DateOfBirth = DateTime.UtcNow.AddYears(-20) });
            await _db.Accounts.AddAsync(new Account { Id = accountId, CustomerId = customerId, AccountNumber = "123", Currency = CurrencyType.USD });
            await _db.SaveChangesAsync();

            var request = new CreateTransactionRequest 
            { 
                CustomerId = customerId, 
                AccountId = accountId, 
                Currency = CurrencyType.EUR 
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateTransactionAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task GetAllTransactionsAsync_ShouldReturnList_WhenAccountExists()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            await _db.Accounts.AddAsync(new Account { Id = accountId, CustomerId = Guid.NewGuid(), AccountNumber = "123", Currency = CurrencyType.USD });
            await _db.Transactions.AddRangeAsync(
                new Transaction { Id = Guid.NewGuid(), AccountId = accountId, Amount = 100, TransactionType = TransactionType.Credit, Currency = CurrencyType.USD },
                new Transaction { Id = Guid.NewGuid(), AccountId = accountId, Amount = -50, TransactionType = TransactionType.Debit, Currency = CurrencyType.USD }
            );
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetAllTransactionsAsync(accountId, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetTransactionByIdAsync_ShouldReturnTransaction_WhenExists()
        {
            // Arrange
            var transactionId = Guid.NewGuid();
            var transaction = new Transaction { Id = transactionId, AccountId = Guid.NewGuid(), Amount = 100, Currency = CurrencyType.USD };
            await _db.Transactions.AddAsync(transaction);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetTransactionByIdAsync(transactionId, CancellationToken.None);

            // Assert
            Assert.Equal(transactionId, result.Id);
        }

        [Fact]
        public async Task TransferAsync_ShouldProcessSuccessfully_WhenValid()
        {
            // Arrange
            var originId = Guid.NewGuid();
            var destinationId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            
            await _db.Accounts.AddRangeAsync(
                new Account { Id = originId, CustomerId = customerId, Balance = 1000, AccountNumber = "1", Currency = CurrencyType.USD },
                new Account { Id = destinationId, CustomerId = customerId, Balance = 500, AccountNumber = "2", Currency = CurrencyType.USD }
            );
            await _db.SaveChangesAsync();

            var request = new CreateTransferRequest
            {
                AccountOriginId = originId,
                AccountDestinationId = destinationId,
                Amount = 200,
                Currency = CurrencyType.USD
            };

            // Act
            var result = await _service.CreateTransferAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            var originAccount = await _db.Accounts.FindAsync(originId);
            var destinationAccount = await _db.Accounts.FindAsync(destinationId);
            Assert.Equal(800, originAccount.Balance);
            Assert.Equal(700, destinationAccount.Balance);

            var transactions = await _db.Transactions.ToListAsync();
            Assert.Equal(2, transactions.Count);
            Assert.Contains(transactions, t => t.AccountId == originId && t.Amount == -200);
            Assert.Contains(transactions, t => t.AccountId == destinationId && t.Amount == 200);
        }

        [Fact]
        public async Task TransferAsync_ShouldThrow_WhenOriginAndDestinationAreSame()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new CreateTransferRequest { AccountOriginId = accountId, AccountDestinationId = accountId };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateTransferAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task TransferAsync_ShouldThrow_WhenAmountIsZeroOrLess()
        {
            // Arrange
            var request = new CreateTransferRequest 
            { 
                AccountOriginId = Guid.NewGuid(), 
                AccountDestinationId = Guid.NewGuid(), 
                Amount = 0 
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateTransferAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task TransferAsync_ShouldThrow_WhenCurrencyMismatch()
        {
            // Arrange
            var originId = Guid.NewGuid();
            var destinationId = Guid.NewGuid();
            
            await _db.Accounts.AddRangeAsync(
                new Account { Id = originId, CustomerId = Guid.NewGuid(), Balance = 1000, AccountNumber = "1", Currency = CurrencyType.USD },
                new Account { Id = destinationId, CustomerId = Guid.NewGuid(), Balance = 500, AccountNumber = "2", Currency = CurrencyType.EUR }
            );
            await _db.SaveChangesAsync();

            var request = new CreateTransferRequest
            {
                AccountOriginId = originId,
                AccountDestinationId = destinationId,
                Amount = 200,
                Currency = CurrencyType.USD
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateTransferAsync(request, CancellationToken.None));
        }
    }
}
