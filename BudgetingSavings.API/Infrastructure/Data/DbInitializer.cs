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
                LastTransactionDate = DateTime.SpecifyKind(new DateTime(2023, 8, 15), DateTimeKind.Utc)
            },
            new Account
            {
                Id = account2,
                AccountNumber = "********5678",
                AccountType = AccountType.Savings,
                Balance = 25000.75m,
                Currency = CurrencyType.NOK,
                CustomerId = customer2,
                LastTransactionDate = DateTime.SpecifyKind(new DateTime(2023, 8, 15), DateTimeKind.Utc)
            },
            new Account
            {
                Id = account3,
                AccountNumber = "********9876",
                AccountType = AccountType.Checking,
                Balance = 2000.50m,
                Currency = CurrencyType.NOK,
                CustomerId = customer3,
                LastTransactionDate = DateTime.SpecifyKind(new DateTime(2023, 8, 15), DateTimeKind.Utc)
            }
        );

        dbContext.Transactions.AddRange(
            new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDateTime = DateTime.SpecifyKind(new DateTime(2023, 8, 15), DateTimeKind.Utc),
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
                TransactionDateTime = DateTime.SpecifyKind(new DateTime(2023, 8, 14), DateTimeKind.Utc),
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
                TransactionDateTime = DateTime.SpecifyKind(new DateTime(2023, 8, 15), DateTimeKind.Utc),
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
                TransactionDateTime = DateTime.SpecifyKind(new DateTime(2023, 8, 14), DateTimeKind.Utc),
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
                TransactionDateTime = DateTime.SpecifyKind(new DateTime(2023, 8, 15), DateTimeKind.Utc),
                Amount = -1000.0m,
                Currency = CurrencyType.NOK,
                AccountId = account3,
                CustomerId = customer3,
                TransactionType = TransactionType.Debit,
                TransactionCategory = TransactionCategory.Rent
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDateTime = DateTime.SpecifyKind(new DateTime(2023, 8, 14), DateTimeKind.Utc),
                Amount = 1500.0m,
                Currency = CurrencyType.NOK,
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
                StartDate = DateTime.SpecifyKind(new DateTime(2023, 1, 1), DateTimeKind.Utc),
                TargetDate = DateTime.SpecifyKind(new DateTime(2023, 7, 1), DateTimeKind.Utc),
                CustomerId = customer1
            },
            new SavingGoal
            {
                Id = Guid.NewGuid(),
                Name = "New Laptop",
                TargetAmount = 15000.0m,
                StartDate = DateTime.SpecifyKind(new DateTime(2023, 5, 10), DateTimeKind.Utc),
                TargetDate = DateTime.SpecifyKind(new DateTime(2023, 12, 31), DateTimeKind.Utc),
                CustomerId = customer2
            },
            new SavingGoal
            {
                Id = Guid.NewGuid(),
                Name = "Emergency Fund",
                TargetAmount = 50000.0m,
                StartDate = DateTime.SpecifyKind(new DateTime(2023, 1, 1), DateTimeKind.Utc),
                TargetDate = DateTime.SpecifyKind(new DateTime(2024, 1, 1), DateTimeKind.Utc),
                CustomerId = customer3
            }
        );

        dbContext.Budgets.AddRange(
            new Budget
            {
                Id = Guid.NewGuid(),
                StartTime = DateTime.SpecifyKind(new DateTime(2023, 1, 1), DateTimeKind.Utc),
                EndTime = DateTime.SpecifyKind(new DateTime(2023, 12, 31), DateTimeKind.Utc),
                LimitAmount = 5000.0m,
                Currency = CurrencyType.NOK,
                CustomerId = customer1
            },
            new Budget
            {
                Id = Guid.NewGuid(),
                StartTime = DateTime.SpecifyKind(new DateTime(2023, 1, 1), DateTimeKind.Utc),
                EndTime = DateTime.SpecifyKind(new DateTime(2023, 12, 31), DateTimeKind.Utc),
                LimitAmount = 10000.0m,
                Currency = CurrencyType.NOK,
                CustomerId = customer2
            },
            new Budget
            {
                Id = Guid.NewGuid(),
                StartTime = DateTime.SpecifyKind(new DateTime(2023, 1, 1), DateTimeKind.Utc),
                EndTime = DateTime.SpecifyKind(new DateTime(2023, 12, 31), DateTimeKind.Utc),
                LimitAmount = 3000.0m,
                Currency = CurrencyType.NOK,
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
                Date = DateTime.UtcNow
            },
            new Reward
            {
                Id = Guid.NewGuid(),
                CustomerId = customer2,
                Amount = 2500.75m,
                Points = 250,
                Redeemed = true,
                RedeemedDate = DateTime.UtcNow,
                Date = DateTime.UtcNow.AddDays(-10)
            }
        );

        dbContext.SaveChanges();
    }
}
