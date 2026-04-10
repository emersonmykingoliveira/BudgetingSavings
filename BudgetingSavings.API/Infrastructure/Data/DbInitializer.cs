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

        dbContext.Accounts.AddRange(
            new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "********1234",
                AccountType = AccountType.Checking,
                Balance = 15000.25m,
                Currency = CurrencyType.NOK,
                Owner = "Alice Johnson"
            },
            new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "********5678",
                AccountType = AccountType.Savings,
                Balance = 25000.75m,
                Currency = CurrencyType.NOK,
                Owner = "Bob Smith"
            },
            new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "********9876",
                AccountType = AccountType.Checking,
                Balance = 2000.50m,
                Currency = CurrencyType.NOK,
                Owner = "Charlie Brown"
            },
            new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "********2109",
                AccountType = AccountType.Savings,
                Balance = 8000.00m,
                Currency = CurrencyType.NOK,
                Owner = "David Wilson"
            }
        );

        dbContext.SaveChanges();
    }
}
