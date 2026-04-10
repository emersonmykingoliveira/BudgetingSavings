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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

            builder.Property(a => a.Owner)
                .IsRequired()
                .HasMaxLength(100);
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
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}