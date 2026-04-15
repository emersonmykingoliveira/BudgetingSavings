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
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(request.Amount * -1, result.Value.Amount);
            var account = await _db.Accounts.FindAsync(accountId);
            Assert.Equal(900, account?.Balance);
            await _rewardService.Received(1).RewardHandlerAsync(Arg.Any<CreateRewardRequest>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task CreateTransactionAsync_ShouldReturnFailure_WhenCustomerDoesNotExist()
        {
            // Arrange
            var request = new CreateTransactionRequest { CustomerId = Guid.NewGuid() };

            // Act
            var result = await _service.CreateTransactionAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Customer does not exist.", result.Error);
        }

        [Fact]
        public async Task CreateTransactionAsync_ShouldReturnFailure_WhenAccountDoesNotExist()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            await _db.Customers.AddAsync(new Customer { Id = customerId, Name = "C1", Email = "c1@e.com", PhoneNumber = "1", DateOfBirth = DateTime.Now });
            await _db.SaveChangesAsync();

            var request = new CreateTransactionRequest { CustomerId = customerId, AccountId = Guid.NewGuid() };

            // Act
            var result = await _service.CreateTransactionAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Account does not exist.", result.Error);
        }

        [Fact]
        public async Task CreateTransactionAsync_ShouldReturnFailure_WhenAccountDoesNotBelongToCustomer()
        {
            // Arrange
            var c1Id = Guid.NewGuid();
            var c2Id = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            await _db.Customers.AddAsync(new Customer { Id = c1Id, Name = "C1", Email = "c1@e.com", PhoneNumber = "1", DateOfBirth = DateTime.Now });
            await _db.Customers.AddAsync(new Customer { Id = c2Id, Name = "C2", Email = "c2@e.com", PhoneNumber = "2", DateOfBirth = DateTime.Now });
            await _db.Accounts.AddAsync(new Account { Id = accountId, CustomerId = c2Id, AccountNumber = "1" });
            await _db.SaveChangesAsync();

            var request = new CreateTransactionRequest { CustomerId = c1Id, AccountId = accountId };

            // Act
            var result = await _service.CreateTransactionAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Account does not belong to the customer.", result.Error);
        }

        [Fact]
        public async Task CreateTransactionAsync_ShouldReturnFailure_WhenCurrencyMismatch()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            await _db.Customers.AddAsync(new Customer { Id = customerId, Name = "C1", Email = "c1@e.com", PhoneNumber = "1", DateOfBirth = DateTime.Now });
            await _db.Accounts.AddAsync(new Account { Id = accountId, CustomerId = customerId, Currency = CurrencyType.USD, AccountNumber = "1" });
            await _db.SaveChangesAsync();

            var request = new CreateTransactionRequest { CustomerId = customerId, AccountId = accountId, Currency = CurrencyType.EUR };

            // Act
            var result = await _service.CreateTransactionAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Transaction currency must match account currency.", result.Error);
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
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
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
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(transactionId, result.Value.Id);
            Assert.Equal(100m, result.Value.Amount);
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
            Assert.Equal(800, originAccount?.Balance);
            Assert.Equal(700, destinationAccount?.Balance);

            var transactions = await _db.Transactions.ToListAsync();
            Assert.Equal(2, transactions.Count);
            Assert.Contains(transactions, t => t.AccountId == originId && t.Amount == -200);
            Assert.Contains(transactions, t => t.AccountId == destinationId && t.Amount == 200);
        }

        [Fact]
        public async Task TransferAsync_ShouldReturnFailure_WhenOriginAndDestinationAreSame()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new CreateTransferRequest { AccountOriginId = accountId, AccountDestinationId = accountId };

            // Act
            var result = await _service.CreateTransferAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Origin and destination accounts cannot be the same.", result.Error);
        }

        [Fact]
        public async Task TransferAsync_ShouldReturnFailure_WhenAmountIsZeroOrLess()
        {
            // Arrange
            var accountOriginId = Guid.NewGuid();
            var accountDestinationId = Guid.NewGuid();
            var request = new CreateTransferRequest { AccountOriginId = accountOriginId, AccountDestinationId = accountDestinationId, Amount = 0 };

            // Act
            var result = await _service.CreateTransferAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Transfer amount must be greater than zero.", result.Error);
        }

        [Fact]
        public async Task TransferAsync_ShouldReturnFailure_WhenCurrencyMismatch()
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

            // Act
            var result = await _service.CreateTransferAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Transfer currency must match both account currencies.", result.Error);
        }

        [Fact]
        public async Task GetAllTransactionsAsync_ShouldReturnFailure_WhenAccountDoesNotExist()
        {
            // Act
            var result = await _service.GetAllTransactionsAsync(Guid.NewGuid(), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Account does not exist.", result.Error);
        }

        [Fact]
        public async Task GetTransactionByIdAsync_ShouldReturnFailure_WhenTransactionDoesNotExist()
        {
            // Act
            var result = await _service.GetTransactionByIdAsync(Guid.NewGuid(), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Transaction does not exist.", result.Error);
        }

        [Fact]
        public async Task TransferAsync_ShouldReturnFailure_WhenInsufficientBalance()
        {
            // Arrange
            var originId = Guid.NewGuid();
            var destinationId = Guid.NewGuid();
            await _db.Accounts.AddRangeAsync(
                new Account { Id = originId, CustomerId = Guid.NewGuid(), Balance = 100, AccountNumber = "1", Currency = CurrencyType.USD },
                new Account { Id = destinationId, CustomerId = Guid.NewGuid(), Balance = 500, AccountNumber = "2", Currency = CurrencyType.USD }
            );
            await _db.SaveChangesAsync();

            var request = new CreateTransferRequest { AccountOriginId = originId, AccountDestinationId = destinationId, Amount = 200, Currency = CurrencyType.USD };

            // Act
            var result = await _service.CreateTransferAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Insufficient balance for transfer.", result.Error);
        }

        [Fact]
        public async Task TransferAsync_ShouldReturnFailure_WhenOriginAccountDoesNotExist()
        {
            // Arrange
            var destId = Guid.NewGuid();
            await _db.Accounts.AddAsync(new Account { Id = destId, CustomerId = Guid.NewGuid(), AccountNumber = "1" });
            await _db.SaveChangesAsync();
            var request = new CreateTransferRequest { AccountOriginId = Guid.NewGuid(), AccountDestinationId = destId, Amount = 10 };

            // Act
            var result = await _service.CreateTransferAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Origin account does not exist.", result.Error);
        }

        [Fact]
        public async Task TransferAsync_ShouldReturnFailure_WhenDestinationAccountDoesNotExist()
        {
            // Arrange
            var originId = Guid.NewGuid();
            await _db.Accounts.AddAsync(new Account { Id = originId, CustomerId = Guid.NewGuid(), AccountNumber = "1" });
            await _db.SaveChangesAsync();
            var request = new CreateTransferRequest { AccountOriginId = originId, AccountDestinationId = Guid.NewGuid(), Amount = 10 };

            // Act
            var result = await _service.CreateTransferAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Destination account does not exist.", result.Error);
        }

        [Fact]
        public async Task CreateTransactionAsync_ShouldHandleRoundUp_WhenValidDebit()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var accountId = Guid.NewGuid();
            var savingsAccountId = Guid.NewGuid();
            await _db.Customers.AddAsync(new Customer { Id = customerId, Name = "Test", Email = "a@a.com", PhoneNumber = "1", DateOfBirth = DateTime.Now });
            await _db.Accounts.AddRangeAsync(
                new Account { Id = accountId, CustomerId = customerId, AccountNumber = "1", Currency = CurrencyType.USD, Balance = 1000, AccountType = AccountType.Checking },
                new Account { Id = savingsAccountId, CustomerId = customerId, AccountNumber = "2", Currency = CurrencyType.USD, Balance = 500, AccountType = AccountType.Savings }
            );
            await _db.SaveChangesAsync();

            var request = new CreateTransactionRequest
            {
                CustomerId = customerId,
                AccountId = accountId,
                Amount = 10.75m,
                Currency = CurrencyType.USD,
                TransactionType = TransactionType.Debit,
                TransactionCategory = TransactionCategory.General
            };

            // Act
            await _service.CreateTransactionAsync(request, CancellationToken.None);

            // Assert
            var checkingAccount = await _db.Accounts.FindAsync(accountId);
            var savingsAccount = await _db.Accounts.FindAsync(savingsAccountId);

            Assert.Equal(989m, checkingAccount?.Balance);
            Assert.Equal(500.25m, savingsAccount?.Balance);
        }
    }
}
