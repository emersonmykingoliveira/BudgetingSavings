using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Infrastructure.Data;

public static class DbInitializer
{
    public static void Initialize(ApiDbContext dbContext)
    {
        dbContext.Database.EnsureCreated();

        if (dbContext.Accounts.Any())
        {
            return;
        }

        var account1 = Guid.NewGuid();
        var account2 = Guid.NewGuid();
        var account3 = Guid.NewGuid();
        var account4 = Guid.NewGuid();

        dbContext.Accounts.AddRange(
            new Account
            {
                Id = account1,
                AccountNumber = "********1234",
                AccountType = AccountType.Checking,
                Balance = 15000.25m,
                Currency = CurrencyType.NOK,
                Owner = "Alice Johnson",
                LastTransactionDate = DateTime.Parse("2023-08-15")
            },
            new Account
            {
                Id = account2,
                AccountNumber = "********5678",
                AccountType = AccountType.Savings,
                Balance = 25000.75m,
                Currency = CurrencyType.NOK,
                Owner = "Bob Smith",
                LastTransactionDate = DateTime.Parse("2023-08-15")
            },
            new Account
            {
                Id = account3,
                AccountNumber = "********9876",
                AccountType = AccountType.Checking,
                Balance = 2000.50m,
                Currency = CurrencyType.NOK,
                Owner = "Charlie Brown",
                LastTransactionDate = DateTime.Parse("2023-08-15")
            },
            new Account
            {
                Id = account4,
                AccountNumber = "********2109",
                AccountType = AccountType.Savings,
                Balance = 8000.00m,
                Currency = CurrencyType.NOK,
                Owner = "David Wilson",
                LastTransactionDate = DateTime.Parse("2023-08-15")
            }
        );

        dbContext.Transactions.AddRange(
            new Transaction
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Parse("2023-08-15"),
                Description = "Grocery Store",
                Amount = -75.5m,
                Currency = CurrencyType.NOK,
                AccountId = account1
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Parse("2023-08-14"),
                Description = "Paycheck Deposit",
                Amount = 2500.0m,
                Currency = CurrencyType.NOK,
                AccountId = account1
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Parse("2023-08-15"),
                Description = "Online Shopping",
                Amount = -320.25m,
                Currency = CurrencyType.NOK,
                AccountId = account2
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Parse("2023-08-14"),
                Description = "Interest Earnings",
                Amount = 50.25m,
                Currency = CurrencyType.NOK,
                AccountId = account2
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Parse("2023-08-15"),
                Description = "Rent Payment",
                Amount = -1000.0m,
                Currency = CurrencyType.NOK,
                AccountId = account3
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Parse("2023-08-14"),
                Description = "Savings Deposit",
                Amount = 1500.0m,
                Currency = CurrencyType.NOK,
                AccountId = account3
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Parse("2023-08-15"),
                Description = "Lunch Out",
                Amount = -45.75m,
                Currency = CurrencyType.NOK,
                AccountId = account4
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                Date = DateTime.Parse("2023-08-14"),
                Description = "Savings Deposit",
                Amount = 1000.0m,
                Currency = CurrencyType.NOK,
                AccountId = account4
            }
        );

        dbContext.SaveChanges();
    }
}
