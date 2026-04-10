using BudgetingSavings.API.Infrastructure.Data;
using BudgetingSavings.API.Infrastructure.Entities;
using BudgetingSavings.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddScoped<IBudgetingService, BudgetingService>();
builder.Services.AddScoped<ISavingGoalsService, SavingGoalsService>();
builder.Services.AddScoped<ITransactionsService, TransactionsService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    dbContext.Database.Migrate();

    if (!dbContext.Accounts.Any())
    {
        dbContext.Accounts.AddRange(
            new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "********1234",
                AccountType = "Checking",
                Balance = 15000.25m,
                Currency = "NOK",
                Owner = "Alice"
            },
            new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "********5678",
                AccountType = "Savings",
                Balance = 25000.75m,
                Currency = "NOK",
                Owner = "Bob"
            },
            new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "********9876",
                AccountType = "Checking",
                Balance = 2000.50m,
                Currency = "NOK",
                Owner = "Charlie"
            },
            new Account
            {
                Id = Guid.NewGuid(),
                AccountNumber = "********2109",
                AccountType = "Savings",
                Balance = 8000.00m,
                Currency = "NOK",
                Owner = "David"
            }
        );

        dbContext.SaveChanges();
    }
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();