using BudgetingSavings.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddScoped<IAccountsService, AccountsService>();
builder.Services.AddScoped<IBudgetingService, BudgetingService>();
builder.Services.AddScoped<ISavingGoalsService, SavingGoalsService>();
builder.Services.AddScoped<ITransactionsService, TransactionsService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();