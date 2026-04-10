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
                .HasMaxLength(50);

            builder.Property(a => a.Balance);

            builder.Property(a => a.Currency)
                .IsRequired()
                .HasMaxLength(10);

            builder.HasOne(a => a.Customer)
                .WithMany(c => c.Accounts)
                .HasForeignKey(a => a.CustomerId)
                .IsRequired();
        });

        modelBuilder.Entity<Transaction>(builder =>
        {
            builder.ToTable("Transactions");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Description)
                .HasMaxLength(250);

            builder.Property(t => t.Amount)
                .IsRequired();

            builder.Property(t => t.Currency)
                .IsRequired()
                .HasMaxLength(10);

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
                .IsRequired();

            builder.HasOne(s => s.Customer)
                .WithMany(c => c.SavingGoals)
                .HasForeignKey(s => s.CustomerId)
                .IsRequired();
        });
    }
}