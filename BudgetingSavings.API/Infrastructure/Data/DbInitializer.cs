using BudgetingSavings.API.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Infrastructure.Data;

public static class DbInitializer
{
    public static void Initialize(ApiDbContext dbContext)
    {
        dbContext.Database.Migrate();

        if (dbContext.Accounts.Any())
        {
            return;
        }

        dbContext.Accounts.AddRange(
            new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "1234.56.78901",
                AccountType = "Checking",
                Balance = 15000.25m,
                Currency = "NOK",
                Owner = "Alice Johnson"
            },
            new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "9876.54.32109",
                AccountType = "Savings",
                Balance = 25000.75m,
                Currency = "NOK",
                Owner = "Bob Smith"
            },
            new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "1122.33.44556",
                AccountType = "Checking",
                Balance = 2000.50m,
                Currency = "NOK",
                Owner = "Charlie Brown"
            },
            new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "5544.33.22110",
                AccountType = "Savings",
                Balance = 8000.00m,
                Currency = "NOK",
                Owner = "David Wilson"
            }
        );

        dbContext.SaveChanges();
    }
}
