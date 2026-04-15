using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Models.Enums;
using BudgetingSavings.API.Models.Requests;
using BudgetingSavings.API.Services;
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
        public async Task CreateAccountAsync_ShouldReturnAccount_WhenValid()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var customer = new Customer { Id = customerId, Name = "Test Customer" };
            await _db.Customers.AddAsync(customer);
            await _db.SaveChangesAsync();

            var request = new CreateAccountRequest
            {
                CustomerId = customerId,
                AccountType = AccountType.Savings,
                Currency = CurrencyType.USD
            };

            // Act
            var result = await _service.CreateAccountAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(customerId, result.Value.CustomerId);
            Assert.Equal(request.AccountType, result.Value.AccountType);
            Assert.Equal(0m, result.Value.Balance);
            Assert.NotNull(result.Value.AccountNumber);
        }

        [Fact]
        public async Task CreateAccountAsync_ShouldReturnFailure_WhenCustomerDoesNotExist()
        {
            // Arrange
            var request = new CreateAccountRequest { CustomerId = Guid.NewGuid() };

            // Act
            var result = await _service.CreateAccountAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Customer does not exist.", result.Error);
        }

        [Fact]
        public async Task CreateAccountAsync_ShouldReturnFailure_WhenAccountTypeAlreadyExists()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            await _db.Customers.AddAsync(new Customer { Id = customerId, Name = "Existing", Email = "e@e.com", PhoneNumber = "1", DateOfBirth = DateTime.Now });
            await _db.Accounts.AddAsync(new Account { Id = Guid.NewGuid(), CustomerId = customerId, AccountType = AccountType.Savings, AccountNumber = "1" });
            await _db.SaveChangesAsync();

            var request = new CreateAccountRequest { CustomerId = customerId, AccountType = AccountType.Savings };

            // Act
            var result = await _service.CreateAccountAsync(request, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Customer already has a Savings account.", result.Error);
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
                AccountType = AccountType.Checking,
                Balance = 100m
            };
            await _db.Accounts.AddAsync(account);
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetAccountByIdAsync(accountId, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(accountId, result.Value.Id);
            Assert.Equal(100m, result.Value.Balance);
        }

        [Fact]
        public async Task GetAccountByIdAsync_ShouldReturnFailure_WhenNotExists()
        {
            // Act
            var result = await _service.GetAccountByIdAsync(Guid.NewGuid(), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Account does not exist.", result.Error);
        }

        [Fact]
        public async Task GetAllAccountsAsync_ShouldReturnList()
        {
            // Arrange
            await _db.Accounts.AddAsync(new Account { Id = Guid.NewGuid(), AccountNumber = "********1111", Currency = CurrencyType.USD });
            await _db.Accounts.AddAsync(new Account { Id = Guid.NewGuid(), AccountNumber = "********2222", Currency = CurrencyType.USD });
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.GetAllAccountsAsync(CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
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
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
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
            await _db.SaveChangesAsync();

            // Assert
            var updatedAccount = await _db.Accounts.FindAsync(accountId);
            Assert.Equal(150m, updatedAccount?.Balance);
        }

        [Fact]
        public async Task UpdateAccountBalanceAsync_ShouldReturnFailure_WhenInsufficientBalance()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            await _db.Accounts.AddAsync(new Account { Id = accountId, Balance = 50m, AccountNumber = "1" });
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.UpdateAccountBalanceAsync(accountId, -100m, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Insufficient balance.", result.Error);
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
        public async Task DeleteAccountAsync_ShouldReturnFailure_WhenBalanceExists()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            await _db.Accounts.AddAsync(new Account { Id = accountId, Balance = 10m, AccountNumber = "1" });
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.DeleteAccountAsync(accountId, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Cannot delete an account that still contains money.", result.Error);
        }

        [Fact]
        public async Task DeleteAccountAsync_ShouldReturnFailure_WhenHasTransactions()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            await _db.Accounts.AddAsync(new Account { Id = accountId, Balance = 0m, AccountNumber = "1" });
            await _db.Transactions.AddAsync(new Transaction { Id = Guid.NewGuid(), AccountId = accountId });
            await _db.SaveChangesAsync();

            // Act
            var result = await _service.DeleteAccountAsync(accountId, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Cannot delete an account with transaction history.", result.Error);
        }

        [Fact]
        public async Task GetAllAccountsForCustomerAsync_ShouldReturnFailure_WhenCustomerDoesNotExist()
        {
            // Act
            var result = await _service.GetAllAccountsForCustomerAsync(Guid.NewGuid(), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Customer does not exist.", result.Error);
        }

        [Fact]
        public async Task UpdateAccountBalanceAsync_ShouldReturnFailure_WhenAmountIsZero()
        {
            // Act
            var result = await _service.UpdateAccountBalanceAsync(Guid.NewGuid(), 0m, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Amount must be different than zero.", result.Error);
        }

        [Fact]
        public async Task UpdateAccountBalanceAsync_ShouldReturnFailure_WhenAccountDoesNotExist()
        {
            // Act
            var result = await _service.UpdateAccountBalanceAsync(Guid.NewGuid(), 100m, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Account does not exist.", result.Error);
        }

        [Fact]
        public async Task DeleteAccountAsync_ShouldReturnFailure_WhenAccountDoesNotExist()
        {
            // Act
            var result = await _service.DeleteAccountAsync(Guid.NewGuid(), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Account does not exist.", result.Error);
        }
    }
}