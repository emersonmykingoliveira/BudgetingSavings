using BudgetingSavings.API.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace BudgetingSavings.API.Infrastructure.Data;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<SavingGoal> SavingGoals => Set<SavingGoal>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<Reward> Rewards => Set<Reward>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(builder =>
        {
            builder.ToTable("Customers");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Email)
                .HasMaxLength(150);

            builder.Property(c => c.PhoneNumber)
                .HasMaxLength(20);

            builder.Property(c => c.DateOfBirth)
                .IsRequired();
        });

        modelBuilder.Entity<Account>(builder =>
        {
            builder.ToTable("Accounts");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.AccountNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.AccountType)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>();

            builder.Property(a => a.Balance)
                .HasColumnType("decimal(18,2)");

            builder.Property(a => a.Currency)
                .IsRequired()
                .HasMaxLength(10)
                .HasConversion<string>();

            builder.Property(a => a.CreatedDate)
                .IsRequired();

            builder.Property(a => a.LastTransactionDate);

            builder.HasOne(a => a.Customer)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.CustomerId)
                .IsRequired();
        });

        modelBuilder.Entity<Transaction>(builder =>
        {
            builder.ToTable("Transactions");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(t => t.TransactionType)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>();

            builder.Property(t => t.TransactionCategory)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion<string>();

            builder.Property(t => t.Currency)
                .IsRequired()
                .HasMaxLength(10)
                .HasConversion<string>();

            builder.Property(t => t.TransactionDateTime)
                .IsRequired();

            builder.HasOne(t => t.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountId)
                .IsRequired();

        });

        modelBuilder.Entity<SavingGoal>(builder =>
        {
            builder.ToTable("SavingGoals");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.TargetAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(s => s.StartDate)
                .IsRequired();

            builder.Property(s => s.TargetDate)
                .IsRequired();

            builder.HasOne(s => s.Customer)
                .WithMany(c => c.SavingGoals)
                .HasForeignKey(s => s.CustomerId)
                .IsRequired();
        });

        modelBuilder.Entity<Budget>(builder =>
        {
            builder.ToTable("Budgets");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.LimitAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(b => b.Currency)
                .IsRequired()
                .HasMaxLength(10)
                .HasConversion<string>();

            builder.Property(b => b.StartTime)
                .IsRequired();

            builder.Property(b => b.EndTime)
                .IsRequired();

            builder.HasOne(b => b.Customer)
                .WithMany(c => c.Budgets)
                .HasForeignKey(b => b.CustomerId)
                .IsRequired();
        });

        modelBuilder.Entity<Reward>(builder =>
        {
            builder.ToTable("Rewards");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Date)
                .IsRequired();

            builder.Property(r => r.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(r => r.Points)
                .IsRequired();

            builder.Property(r => r.Redeemed)
                .IsRequired();

            builder.Property(r => r.CashBack)
                .HasColumnType("decimal(18,2)");

            builder.Property(r => r.RedeemedDate);

            builder.HasOne(r => r.Customer)
                .WithMany(c => c.Rewards)
                .HasForeignKey(r => r.CustomerId)
                .IsRequired();
        });
    }
}