using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.Shared.Models.Enums;
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
            new Customer { Id = customer1, Name = "Alice Johnson", Email = "alice@example.com", DateOfBirth = new DateTime(1990, 5, 20) },
            new Customer { Id = customer2, Name = "Bob Smith", Email = "bob@example.com", DateOfBirth = new DateTime(1985, 12, 10) },
            new Customer { Id = customer3, Name = "Charlie Brown", Email = "charlie@example.com", DateOfBirth = new DateTime(1995, 3, 15) }
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
            }
        );

        dbContext.Transactions.AddRange(
            new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDateTime = DateTime.Parse("2023-08-15"),
                Amount = -75.5m,
                Currency = CurrencyType.NOK,
                AccountId = account1,
                TransactionType = TransactionType.Debit,
                TransactionCategory = TransactionCategory.Groceries
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDateTime = DateTime.Parse("2023-08-14"),
                Amount = 2500.0m,
                Currency = CurrencyType.NOK,
                AccountId = account1,
                TransactionType = TransactionType.Credit,
                TransactionCategory = TransactionCategory.General
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDateTime = DateTime.Parse("2023-08-15"),
                Amount = -320.25m,
                Currency = CurrencyType.NOK,
                AccountId = account2,
                TransactionType = TransactionType.Debit,
                TransactionCategory = TransactionCategory.Entertainment
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDateTime = DateTime.Parse("2023-08-14"),
                Amount = 50.25m,
                Currency = CurrencyType.NOK,
                AccountId = account2,
                TransactionType = TransactionType.Credit,
                TransactionCategory = TransactionCategory.Savings
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDateTime = DateTime.Parse("2023-08-15"),
                Amount = -1000.0m,
                Currency = CurrencyType.NOK,
                AccountId = account3,
                TransactionType = TransactionType.Debit,
                TransactionCategory = TransactionCategory.Rent
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                TransactionDateTime = DateTime.Parse("2023-08-14"),
                Amount = 1500.0m,
                Currency = CurrencyType.NOK,
                AccountId = account3,
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
