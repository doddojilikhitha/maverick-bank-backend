using MaverickBank.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace MaverickBank.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<Loan> Loans => Set<Loan>();
        public DbSet<LoanProduct> LoanProducts => Set<LoanProduct>();
        public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>()
                .Ignore(u => u.Age);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.User)
                .WithMany(u => u.Accounts)
                .HasForeignKey(a => a.UserId);
            modelBuilder.Entity<Account>()
                .HasIndex(a => a.AccountNumber).IsUnique();
            modelBuilder.Entity<Account>()
                .Property(a => a.Balance)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.ToAccount)
                .WithMany()
                .HasForeignKey(t => t.ToAccountId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Loan>()
                .HasOne(l => l.User)
                .WithMany(u => u.Loans)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.LoanProduct)
                .WithMany(lp => lp.Loans)
                .HasForeignKey(l => l.LoanProductId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Loan>()
                .HasOne(l => l.Account)
                .WithMany(a => a.Loans)
                .HasForeignKey(l => l.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Loan>()
                .Property(l => l.AmountApplied)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Beneficiary>()
                .HasOne(b => b.User)
                .WithMany(u => u.Beneficiaries)
                .HasForeignKey(b => b.UserId);

            modelBuilder.Entity<LoanProduct>()
                .Property(lp => lp.LoanAmount)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<LoanProduct>()
                .Property(lp => lp.InterestRate)
                .HasColumnType("decimal(5,2)");

            // Seed loan products
            modelBuilder.Entity<LoanProduct>().HasData(
                new LoanProduct { LoanProductId = 1, ProductName = "Home Loan", LoanAmount = 5000000, InterestRate = 8.5m, TenureMonths = 240, IsActive = true },
                new LoanProduct { LoanProductId = 2, ProductName = "Car Loan", LoanAmount = 1000000, InterestRate = 9.0m, TenureMonths = 60, IsActive = true },
                new LoanProduct { LoanProductId = 3, ProductName = "Personal Loan", LoanAmount = 500000, InterestRate = 12.0m, TenureMonths = 36, IsActive = true },
                new LoanProduct { LoanProductId = 4, ProductName = "Education Loan", LoanAmount = 2000000, InterestRate = 7.5m, TenureMonths = 120, IsActive = true }
            );
        }
    }
}