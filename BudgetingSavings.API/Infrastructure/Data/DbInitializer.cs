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

        var account1 = Guid.Parse("3e8bd3ed-b0fb-49db-b332-3815686054ee");
        var account2 = Guid.Parse("a3d8ae09-b4c7-4c1f-9bac-166bb26b0d2b");
        var account3 = Guid.Parse("af79b5af-cf9c-4ade-996e-92538089a180");
        var account4 = Guid.Parse("34b04500-2eb3-4f7a-8e9f-c2f0c9d3df32");

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
