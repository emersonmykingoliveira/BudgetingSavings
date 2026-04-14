using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Services;
using BudgetingSavings.Shared.Models.Requests;
using BudgetingSavings.Shared.Models.Responses;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace BudgetingSavings.Tests.UnitTests
{
    public class AccountServiceUnitTests : IDisposable
    {
        private readonly ApiDbContext _db;
        private readonly IValidator<CreateAccountRequest> _validator;
        private readonly IAccountService _service;

        public AccountServiceUnitTests()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _db = new ApiDbContext(options);
            _validator = Substitute.For<IValidator<CreateAccountRequest>>();
            _service = new AccountService(_db, _validator);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        [Fact]
        public async Task CreateAccountAsync_ShouldReturnAccount_WhenValidRequest()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer { Id = customerId, Name = "Test Customer" };
            await _db.Customers.AddAsync(customer);
            await _db.SaveChangesAsync();

            var request = new CreateAccountRequest
            {
                CustomerId = customerId,
                AccountType = API.Models.Enums.AccountType.Savings,
                Currency = API.Models.Enums.CurrencyType.USD
            };

            // Act
            var result = await _service.CreateAccountAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customerId, result.CustomerId);
            Assert.Equal(request.AccountType, result.AccountType);
            Assert.Equal(0m, result.Balance);
        }

        [Fact]
        public async Task CreateAccountAsync_ShouldThrowException_WhenCustomerDoesNotExist()
        {
            // Arrange
            var request = new CreateAccountRequest
            {
                CustomerId = Guid.NewGuid(),
                AccountType = API.Models.Enums.AccountType.Savings
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateAccountAsync(request, CancellationToken.None));
            Assert.Equal("Customer does not exist.", exception.Message);
        }

        [Fact]
        public async Task CreateAccountAsync_ShouldThrowException_WhenAccountTypeAlreadyExists()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer { Id = customerId, Name = "Test Customer" };
            await _db.Customers.AddAsync(customer);
            
            var existingAccount = new Account
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                AccountType = API.Models.Enums.AccountType.Savings,
                AccountNumber = "********1234"
            };
            await _db.Accounts.AddAsync(existingAccount);
            await _db.SaveChangesAsync();

            var request = new CreateAccountRequest
            {
                CustomerId = customerId,
                AccountType = API.Models.Enums.AccountType.Savings
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.CreateAccountAsync(request, CancellationToken.None));
            Assert.Equal($"Customer already has a {request.AccountType} account.", exception.Message);
        }

        [Fact]
        public async Task GetAccountByIdAsync_ShouldReturnAccount_WhenExists()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account
            {
                Id = accountId,
                CustomerId = Guid.NewGuid(),
                AccountNumber = "********5678",
                AccountType = API.Models.Enums.AccountType.Checking,
                Balance = 100m
            };
            await _db.Accounts.AddAsync(account);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetAccountByIdAsync(accountId, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(accountId, result.Id);
            Assert.Equal(100m, result.Balance);
        }

        [Fact]
        public async Task GetAccountByIdAsync_ShouldThrowException_WhenNotExists()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.GetAccountByIdAsync(Guid.NewGuid(), CancellationToken.None));
            Assert.Equal("Account does not exist.", exception.Message);
        }

        [Fact]
        public async Task GetAllAccountsAsync_ShouldReturnList()
        {
            // Arrange
            await _db.Accounts.AddAsync(new Account { Id = Guid.NewGuid(), AccountNumber = "********1111" });
            await _db.Accounts.AddAsync(new Account { Id = Guid.NewGuid(), AccountNumber = "********2222" });
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetAllAccountsAsync(CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetAllAccountsForCustomerAsync_ShouldReturnAccounts_WhenCustomerExists()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            await _db.Customers.AddAsync(new Customer { Id = customerId, Name = "Test" });
            await _db.Accounts.AddAsync(new Account { Id = Guid.NewGuid(), CustomerId = customerId, AccountNumber = "********1111" });
            await _db.Accounts.AddAsync(new Account { Id = Guid.NewGuid(), CustomerId = customerId, AccountNumber = "********2222" });
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetAllAccountsForCustomerAsync(customerId, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task UpdateAccountBalanceAsync_ShouldUpdate_WhenValid()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId, Balance = 100m, AccountNumber = "1" };
            await _db.Accounts.AddAsync(account);
            await _db.SaveChangesAsync();

            // Act
            await _service.UpdateAccountBalanceAsync(accountId, 50m, CancellationToken.None);

            // Assert
            var updatedAccount = await _db.Accounts.FindAsync(accountId);
            Assert.Equal(150m, updatedAccount.Balance);
        }

        [Fact]
        public async Task UpdateAccountBalanceAsync_ShouldThrow_WhenInsufficientBalance()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId, Balance = 50m, AccountNumber = "1" };
            await _db.Accounts.AddAsync(account);
            await _db.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.UpdateAccountBalanceAsync(accountId, -100m, CancellationToken.None));
        }

        [Fact]
        public async Task DeleteAccountAsync_ShouldRemove_WhenNoBalanceAndNoTransactions()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId, Balance = 0m, AccountNumber = "1" };
            await _db.Accounts.AddAsync(account);
            await _db.SaveChangesAsync();

            // Act
            await _service.DeleteAccountAsync(accountId, CancellationToken.None);

            // Assert
            var deletedAccount = await _db.Accounts.FindAsync(accountId);
            Assert.Null(deletedAccount);
        }

        [Fact]
        public async Task DeleteAccountAsync_ShouldThrow_WhenBalanceExists()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId, Balance = 10m, AccountNumber = "1" };
            await _db.Accounts.AddAsync(account);
            await _db.SaveChangesAsync();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.DeleteAccountAsync(accountId, CancellationToken.None));
            Assert.Equal("Cannot delete an account that still contains money.", exception.Message);
        }

        [Fact]
        public async Task DeleteAccountAsync_ShouldThrow_WhenHasTransactions()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId, Balance = 0m, AccountNumber = "1" };
            var transaction = new Transaction { Id = Guid.NewGuid(), AccountId = accountId, Amount = 100m, TransactionDateTime = DateTime.UtcNow };
            await _db.Accounts.AddAsync(account);
            await _db.Transactions.AddAsync(transaction);
            await _db.SaveChangesAsync();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.DeleteAccountAsync(accountId, CancellationToken.None));
            Assert.Equal("Cannot delete an account with transaction history.", exception.Message);
        }

        [Fact]
        public async Task GetAllAccountsForCustomerAsync_ShouldThrow_WhenCustomerDoesNotExist()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.GetAllAccountsForCustomerAsync(Guid.NewGuid(), CancellationToken.None));
            Assert.Equal("Customer does not exist.", exception.Message);
        }

        [Fact]
        public async Task UpdateAccountBalanceAsync_ShouldThrow_WhenAmountIsZero()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new Account { Id = accountId, Balance = 100m, AccountNumber = "1" };
            await _db.Accounts.AddAsync(account);
            await _db.SaveChangesAsync();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _service.UpdateAccountBalanceAsync(accountId, 0m, CancellationToken.None));
            Assert.Equal("Amount must be different than zero.", exception.Message);
        }
    }
}
