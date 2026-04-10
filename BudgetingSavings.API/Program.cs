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
    DbInitializer.Initialize(dbContext);
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();