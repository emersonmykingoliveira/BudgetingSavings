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
            return;

        var customer1 = Guid.Parse("f964035a-73d8-4f8e-a9d0-111111111111");
        var customer2 = Guid.Parse("f964035a-73d8-4f8e-a9d0-222222222222");
        var customer3 = Guid.Parse("f964035a-73d8-4f8e-a9d0-333333333333");
        var customer4 = Guid.Parse("f964035a-73d8-4f8e-a9d0-444444444444");

        dbContext.Customers.AddRange(
            new Customer { Id = customer1, Name = "Alice Johnson", Email = "alice@example.com", DateOfBirth = new DateTime(1990, 5, 20) },
            new Customer { Id = customer2, Name = "Bob Smith", Email = "bob@example.com", DateOfBirth = new DateTime(1985, 12, 10) },
            new Customer { Id = customer3, Name = "Charlie Brown", Email = "charlie@example.com", DateOfBirth = new DateTime(1995, 3, 15) },
            new Customer { Id = customer4, Name = "David Wilson", Email = "david@example.com", DateOfBirth = new DateTime(1982, 7, 30) }
        );

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
                CustomerId = customer1,
                LastTransactionDate = DateTime.Parse("2023-08-15")
            },
            new Account
            {
                Id = account2,
                AccountNumber = "********5678",
                AccountType = AccountType.Savings,
                Balance = 25000.75m,
                Currency = CurrencyType.NOK,
                CustomerId = customer2,
                LastTransactionDate = DateTime.Parse("2023-08-15")
            },
            new Account
            {
                Id = account3,
                AccountNumber = "********9876",
                AccountType = AccountType.Checking,
                Balance = 2000.50m,
                Currency = CurrencyType.NOK,
                CustomerId = customer3,
                LastTransactionDate = DateTime.Parse("2023-08-15")
            },
            new Account
            {
                Id = account4,
                AccountNumber = "********2109",
                AccountType = AccountType.Savings,
                Balance = 8000.00m,
                Currency = CurrencyType.NOK,
                CustomerId = customer4,
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

        dbContext.SavingGoals.AddRange(
            new SavingGoal
            {
                Id = Guid.NewGuid(),
                Name = "Summer Vacation",
                TargetAmount = 20000.0m,
                StartDate = DateTime.Parse("2023-01-01"),
                TargetDate = DateTime.Parse("2023-07-01"),
                CustomerId = customer1
            },
            new SavingGoal
            {
                Id = Guid.NewGuid(),
                Name = "New Laptop",
                TargetAmount = 15000.0m,
                StartDate = DateTime.Parse("2023-05-10"),
                TargetDate = DateTime.Parse("2023-12-31"),
                CustomerId = customer2
            },
            new SavingGoal
            {
                Id = Guid.NewGuid(),
                Name = "Emergency Fund",
                TargetAmount = 50000.0m,
                StartDate = DateTime.Parse("2023-01-01"),
                TargetDate = DateTime.Parse("2024-01-01"),
                CustomerId = customer3
            }
        );

        dbContext.SaveChanges();
    }
}
