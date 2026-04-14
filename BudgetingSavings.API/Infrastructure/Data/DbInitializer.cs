using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Infrastructure.Data;

public static class DbInitializer
{
    public static void Initialize(ApiDbContext dbContext)
    {
        dbContext.Database.EnsureCreated();

        if (dbContext.Accounts.Any())
            return;

        var customer1 = Guid.Parse("21429693-768b-4a76-9364-ca9441c434c9");
        var customer2 = Guid.Parse("87fb436a-533c-4a03-a79d-98aef377fddd");
        var customer3 = Guid.Parse("09481c1d-111f-46d8-8c10-e1bce9ce95b6");

        dbContext.Customers.AddRange(
            new Customer { Id = customer1, Name = "Alice Johnson", Email = "alice@example.com", PhoneNumber = "+47 123 45 678", DateOfBirth = new DateTime(1990, 5, 20, 0, 0, 0, DateTimeKind.Utc) },
            new Customer { Id = customer2, Name = "Bob Smith", Email = "bob@example.com", PhoneNumber = "+47 876 54 321", DateOfBirth = new DateTime(1985, 12, 10, 0, 0, 0, DateTimeKind.Utc) },
            new Customer { Id = customer3, Name = "Charlie Brown", Email = "charlie@example.com", PhoneNumber = "+47 555 66 777", DateOfBirth = new DateTime(1995, 3, 15, 0, 0, 0, DateTimeKind.Utc) }
        );

        var account1 = Guid.Parse("3e8bd3ed-b0fb-49db-b332-3815686054ee");
        var account1Savings = Guid.Parse("7c9e0b8e-1f2a-43b3-8c10-e1bce9ce95b6");
        var account2 = Guid.Parse("a3d8ae09-b4c7-4c1f-9bac-166bb26b0d2b");
        var account3 = Guid.Parse("af79b5af-cf9c-4ade-996e-92538089a180");

        dbContext.Accounts.AddRange(
            new Account
            {
                Id = account1,
                AccountNumber = "********1234",
                AccountType = AccountType.Checking,
                Balance = 15000.25m,
                Currency = CurrencyType.NOK,
                CustomerId = customer1,
                CreatedDate = DateTime.UtcNow.AddMonths(-6),
                LastTransactionDate = DateTime.UtcNow.AddDays(-1)
            },
            new Account
            {
                Id = account1Savings,
                AccountNumber = "********4321",
                AccountType = AccountType.Savings,
                Balance = 5000.00m,
                Currency = CurrencyType.NOK,
                CustomerId = customer1,
                CreatedDate = DateTime.UtcNow.AddMonths(-6),
                LastTransactionDate = DateTime.UtcNow.AddDays(-5)
            },
            new Account
            {
                Id = account2,
                AccountNumber = "********5678",
                AccountType = AccountType.Savings,
                Balance = 25000.75m,
                Currency = CurrencyType.NOK,
                CustomerId = customer2,
                CreatedDate = DateTime.UtcNow.AddMonths(-12),
                LastTransactionDate = DateTime.UtcNow.AddDays(-2)
            },
            new Account
            {
                Id = account3,
                AccountNumber = "********9876",
                AccountType = AccountType.Checking,
                Balance = 2000.50m,
                Currency = CurrencyType.USD,
                CustomerId = customer3,
                CreatedDate = DateTime.UtcNow.AddMonths(-3),
                LastTransactionDate = DateTime.UtcNow.AddDays(-3)
            }
        );

        dbContext.Transactions.AddRange(
            new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDateTime = DateTime.UtcNow.AddDays(-1),
                Amount = -75.5m,
                Currency = CurrencyType.NOK,
                AccountId = account1,
                CustomerId = customer1,
                TransactionType = TransactionType.Debit,
                TransactionCategory = TransactionCategory.Groceries
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDateTime = DateTime.UtcNow.AddDays(-2),
                Amount = 2500.0m,
                Currency = CurrencyType.NOK,
                AccountId = account1,
                CustomerId = customer1,
                TransactionType = TransactionType.Credit,
                TransactionCategory = TransactionCategory.General
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDateTime = DateTime.UtcNow.AddDays(-2),
                Amount = -320.25m,
                Currency = CurrencyType.NOK,
                AccountId = account2,
                CustomerId = customer2,
                TransactionType = TransactionType.Debit,
                TransactionCategory = TransactionCategory.Entertainment
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDateTime = DateTime.UtcNow.AddDays(-3),
                Amount = 50.25m,
                Currency = CurrencyType.NOK,
                AccountId = account2,
                CustomerId = customer2,
                TransactionType = TransactionType.Credit,
                TransactionCategory = TransactionCategory.Savings
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDateTime = DateTime.UtcNow.AddDays(-3),
                Amount = -1000.0m,
                Currency = CurrencyType.USD,
                AccountId = account3,
                CustomerId = customer3,
                TransactionType = TransactionType.Debit,
                TransactionCategory = TransactionCategory.Rent
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDateTime = DateTime.UtcNow.AddDays(-4),
                Amount = 1500.0m,
                Currency = CurrencyType.USD,
                AccountId = account3,
                CustomerId = customer3,
                TransactionType = TransactionType.Credit,
                TransactionCategory = TransactionCategory.General
            }
        );

        dbContext.SavingGoals.AddRange(
            new SavingGoal
            {
                Id = Guid.NewGuid(),
                Name = "Summer Vacation",
                TargetAmount = 20000.0m,
                StartDate = DateTime.UtcNow.AddMonths(-3),
                TargetDate = DateTime.UtcNow.AddMonths(3),
                CustomerId = customer1
            },
            new SavingGoal
            {
                Id = Guid.NewGuid(),
                Name = "New Laptop",
                TargetAmount = 15000.0m,
                StartDate = DateTime.UtcNow.AddMonths(-1),
                TargetDate = DateTime.UtcNow.AddMonths(5),
                CustomerId = customer2
            },
            new SavingGoal
            {
                Id = Guid.NewGuid(),
                Name = "Emergency Fund",
                TargetAmount = 50000.0m,
                StartDate = DateTime.UtcNow.AddMonths(-6),
                TargetDate = DateTime.UtcNow.AddMonths(12),
                CustomerId = customer3
            }
        );

        dbContext.Budgets.AddRange(
            new Budget
            {
                Id = Guid.NewGuid(),
                StartTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1).AddDays(-1),
                LimitAmount = 5000.0m,
                Currency = CurrencyType.NOK,
                CustomerId = customer1
            },
            new Budget
            {
                Id = Guid.NewGuid(),
                StartTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1).AddDays(-1),
                LimitAmount = 10000.0m,
                Currency = CurrencyType.NOK,
                CustomerId = customer2
            },
            new Budget
            {
                Id = Guid.NewGuid(),
                StartTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1).AddDays(-1),
                LimitAmount = 3000.0m,
                Currency = CurrencyType.USD,
                CustomerId = customer3
            }
        );

        dbContext.Rewards.AddRange(
            new Reward
            {
                Id = Guid.NewGuid(),
                CustomerId = customer1,
                Amount = 1500.25m,
                Points = 150,
                Redeemed = false,
                CashBack = 0,
                Date = DateTime.UtcNow
            },
            new Reward
            {
                Id = Guid.NewGuid(),
                CustomerId = customer2,
                Amount = 2500.75m,
                Points = 250,
                Redeemed = true,
                RedeemedDate = DateTime.UtcNow.AddDays(-5),
                CashBack = 25.00m,
                Date = DateTime.UtcNow.AddDays(-10)
            },
            new Reward
            {
                Id = Guid.NewGuid(),
                CustomerId = customer3,
                Amount = 500.00m,
                Points = 50,
                Redeemed = false,
                CashBack = 0,
                Date = DateTime.UtcNow.AddDays(-2)
            }
        );

        dbContext.SaveChanges();
    }
}
