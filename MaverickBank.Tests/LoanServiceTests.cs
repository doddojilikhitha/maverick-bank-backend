using MaverickBank.Core.DTOs;
using MaverickBank.Core.Entities;
using MaverickBank.Infrastructure.Data;
using MaverickBank.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace MaverickBank.Tests
{
    [TestFixture]
    public class LoanServiceTests
    {
        private AppDbContext GetDb(string name) =>
            new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(name).Options);

        // ── Helper: seeds the minimum required data ──────────
        private async Task SeedBaseDataAsync(AppDbContext db,
            decimal accountBalance = 50000,
            string accountStatus = "Active")
        {
            db.LoanProducts.Add(new LoanProduct
            {
                LoanProductId = 1,
                ProductName = "Home Loan",
                LoanAmount = 500000,
                InterestRate = 8.5m,
                TenureMonths = 120,
                IsActive = true
            });

            db.Accounts.Add(new Account
            {
                AccountId = 1,
                UserId = 1,
                AccountNumber = "ACC001",
                Balance = accountBalance,
                Status = accountStatus
            });

            await db.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────
        // APPLY LOAN TESTS
        // ─────────────────────────────────────────────────────

        [Test]
        public async Task ApplyLoan_ShouldSucceed_WhenValidData()
        {
            using var db = GetDb("Loan_Apply_Success");
            await SeedBaseDataAsync(db);

            var service = new LoanService(db);
            var result = await service.ApplyLoanAsync(new LoanApplyDTO
            {
                UserId = 1,
                AccountId = 1,
                LoanProductId = 1,
                AmountApplied = 300000,
                Purpose = "Home renovation"
            });

            Assert.That(result, Is.True);
            Assert.That(await db.Loans.CountAsync(), Is.EqualTo(1));
            Assert.That((await db.Loans.FirstAsync()).Status, Is.EqualTo("Pending"));
        }

        [Test]
        public async Task ApplyLoan_ShouldFail_WhenAccountNotFound()
        {
            using var db = GetDb("Loan_Apply_NoAcc");
            db.LoanProducts.Add(new LoanProduct
            {
                LoanProductId = 1,
                ProductName = "Car Loan",
                LoanAmount = 100000,
                InterestRate = 9.0m,
                TenureMonths = 60,
                IsActive = true
            });
            await db.SaveChangesAsync();

            var service = new LoanService(db);
            var result = await service.ApplyLoanAsync(new LoanApplyDTO
            {
                UserId = 1,
                AccountId = 99,
                LoanProductId = 1,
                AmountApplied = 50000
            });

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ApplyLoan_ShouldFail_WhenProductInactive()
        {
            using var db = GetDb("Loan_Apply_Inactive");
            db.Accounts.Add(new Account
            {
                AccountId = 1,
                UserId = 1,
                AccountNumber = "ACC001",
                Status = "Active",
                Balance = 1000
            });
            db.LoanProducts.Add(new LoanProduct
            {
                LoanProductId = 1,
                ProductName = "Old Loan",
                LoanAmount = 50000,
                InterestRate = 7.5m,
                TenureMonths = 24,
                IsActive = false   // ← inactive
            });
            await db.SaveChangesAsync();

            var service = new LoanService(db);
            var result = await service.ApplyLoanAsync(new LoanApplyDTO
            {
                UserId = 1,
                AccountId = 1,
                LoanProductId = 1,
                AmountApplied = 30000
            });

            Assert.That(result, Is.False);
        }

        // ─────────────────────────────────────────────────────
        // GET LOANS TESTS
        // ─────────────────────────────────────────────────────

        [Test]
        public async Task GetMyLoans_ShouldReturn_OnlyCurrentUserLoans()
        {
            using var db = GetDb("Loan_GetMy");

            // Must seed LoanProduct first for Include() to work
            db.LoanProducts.Add(new LoanProduct
            {
                LoanProductId = 1,
                ProductName = "Home Loan",
                LoanAmount = 500000,
                InterestRate = 8.5m,
                TenureMonths = 120,
                IsActive = true
            });

            db.Accounts.AddRange(
                new Account { AccountId = 1, UserId = 1, AccountNumber = "ACC001", Status = "Active", Balance = 5000 },
                new Account { AccountId = 2, UserId = 2, AccountNumber = "ACC002", Status = "Active", Balance = 5000 }
            );

            db.Loans.AddRange(
                new Loan { LoanId = 1, UserId = 1, LoanProductId = 1, AccountId = 1, AmountApplied = 100000, Status = "Pending" },
                new Loan { LoanId = 2, UserId = 1, LoanProductId = 1, AccountId = 1, AmountApplied = 200000, Status = "Approved" },
                new Loan { LoanId = 3, UserId = 2, LoanProductId = 1, AccountId = 2, AmountApplied = 50000, Status = "Pending" }
            );
            await db.SaveChangesAsync();

            var service = new LoanService(db);
            var result = await service.GetMyLoansAsync(userId: 1);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(l => l.UserId == 1), Is.True);
        }

        [Test]
        public async Task GetAllLoans_ShouldReturn_AllLoans()
        {
            using var db = GetDb("Loan_GetAll");

            // Must seed LoanProduct first for Include() to work
            db.LoanProducts.Add(new LoanProduct
            {
                LoanProductId = 1,
                ProductName = "Home Loan",
                LoanAmount = 500000,
                InterestRate = 8.5m,
                TenureMonths = 120,
                IsActive = true
            });

            db.Accounts.AddRange(
                new Account { AccountId = 1, UserId = 1, AccountNumber = "ACC001", Status = "Active", Balance = 5000 },
                new Account { AccountId = 2, UserId = 2, AccountNumber = "ACC002", Status = "Active", Balance = 5000 },
                new Account { AccountId = 3, UserId = 3, AccountNumber = "ACC003", Status = "Active", Balance = 5000 }
            );

            db.Loans.AddRange(
                new Loan { LoanId = 1, UserId = 1, LoanProductId = 1, AccountId = 1, AmountApplied = 100000, Status = "Pending" },
                new Loan { LoanId = 2, UserId = 2, LoanProductId = 1, AccountId = 2, AmountApplied = 200000, Status = "Approved" },
                new Loan { LoanId = 3, UserId = 3, LoanProductId = 1, AccountId = 3, AmountApplied = 50000, Status = "Disbursed" }
            );
            await db.SaveChangesAsync();

            var service = new LoanService(db);
            var result = await service.GetAllLoansAsync();

            Assert.That(result.Count, Is.EqualTo(3));
        }

        // ─────────────────────────────────────────────────────
        // APPROVE / REJECT TESTS
        // ─────────────────────────────────────────────────────

        [Test]
        public async Task ApproveLoan_ShouldSucceed_WhenCreditWorthy()
        {
            using var db = GetDb("Loan_Approve_CreditWorthy");
            await SeedBaseDataAsync(db, accountBalance: 50000);

            // Seed loan
            db.Loans.Add(new Loan
            {
                LoanId = 1,
                UserId = 1,
                LoanProductId = 1,
                AccountId = 1,
                AmountApplied = 100000,
                Status = "Pending"
            });

            // Seed transactions so creditworthiness passes:
            // Balance   = 50000 > 5000                ✅
            // Inbound   = 60000 > 10% of 100000=10000 ✅
            // NetCash   = 60000 - 5000 = 55000 > 0    ✅
            db.Transactions.AddRange(
                new Transaction { AccountId = 1, Type = "Deposit", Amount = 60000 },
                new Transaction { AccountId = 1, Type = "Withdrawal", Amount = 5000 }
            );
            await db.SaveChangesAsync();

            var service = new LoanService(db);
            var result = await service.ApproveLoanAsync(loanId: 1, reviewedByEmployeeId: 5);

            Assert.That(result, Is.True);
            var loan = await db.Loans.FindAsync(1);
            Assert.That(loan!.Status, Is.EqualTo("Approved"));
            Assert.That(loan.ReviewedBy, Is.EqualTo(5));
            Assert.That(loan.ReviewedOn, Is.Not.Null);
        }

        [Test]
        public async Task ApproveLoan_ShouldFail_WhenNotCreditWorthy()
        {
            using var db = GetDb("Loan_Approve_NotCreditWorthy");
            // Low balance — fails creditworthiness
            await SeedBaseDataAsync(db, accountBalance: 100);

            db.Loans.Add(new Loan
            {
                LoanId = 1,
                UserId = 1,
                LoanProductId = 1,
                AccountId = 1,
                AmountApplied = 100000,
                Status = "Pending"
            });
            await db.SaveChangesAsync();

            var service = new LoanService(db);
            var result = await service.ApproveLoanAsync(loanId: 1, reviewedByEmployeeId: 5);

            Assert.That(result, Is.False);
            // Auto-rejected
            var loan = await db.Loans.FindAsync(1);
            Assert.That(loan!.Status, Is.EqualTo("Rejected"));
        }

        [Test]
        public async Task ApproveLoan_ShouldFail_WhenAlreadyApproved()
        {
            using var db = GetDb("Loan_Approve_Already");
            await SeedBaseDataAsync(db);

            db.Loans.Add(new Loan
            {
                LoanId = 1,
                UserId = 1,
                LoanProductId = 1,
                AccountId = 1,
                AmountApplied = 100000,
                Status = "Approved"   // already approved
            });
            await db.SaveChangesAsync();

            var service = new LoanService(db);
            var result = await service.ApproveLoanAsync(1, 5);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task RejectLoan_ShouldSucceed_WhenPending()
        {
            using var db = GetDb("Loan_Reject");
            await SeedBaseDataAsync(db);

            db.Loans.Add(new Loan
            {
                LoanId = 1,
                UserId = 1,
                LoanProductId = 1,
                AccountId = 1,
                AmountApplied = 75000,
                Status = "Pending"
            });
            await db.SaveChangesAsync();

            var service = new LoanService(db);
            var result = await service.RejectLoanAsync(loanId: 1, reviewedByEmployeeId: 3);

            Assert.That(result, Is.True);
            var loan = await db.Loans.FindAsync(1);
            Assert.That(loan!.Status, Is.EqualTo("Rejected"));
            Assert.That(loan.ReviewedBy, Is.EqualTo(3));
        }

        // ─────────────────────────────────────────────────────
        // DISBURSE TESTS
        // ─────────────────────────────────────────────────────

        [Test]
        public async Task DisburseLoan_ShouldCreditAmount_ToAccount()
        {
            using var db = GetDb("Loan_Disburse");
            await SeedBaseDataAsync(db, accountBalance: 1000);

            db.Loans.Add(new Loan
            {
                LoanId = 1,
                UserId = 1,
                LoanProductId = 1,
                AccountId = 1,
                AmountApplied = 100000,
                Status = "Approved"
            });
            await db.SaveChangesAsync();

            var service = new LoanService(db);
            var result = await service.DisburseLoanAsync(loanId: 1);

            Assert.That(result, Is.True);
            Assert.That((await db.Accounts.FindAsync(1))!.Balance, Is.EqualTo(101000));

            var loan = await db.Loans.FindAsync(1);
            Assert.That(loan!.Status, Is.EqualTo("Disbursed"));
            Assert.That(loan.DisbursedOn, Is.Not.Null);
        }

        [Test]
        public async Task DisburseLoan_ShouldFail_WhenNotApproved()
        {
            using var db = GetDb("Loan_Disburse_NotApproved");
            await SeedBaseDataAsync(db);

            db.Loans.Add(new Loan
            {
                LoanId = 1,
                UserId = 1,
                LoanProductId = 1,
                AccountId = 1,
                AmountApplied = 50000,
                Status = "Pending"   // not approved
            });
            await db.SaveChangesAsync();

            var service = new LoanService(db);
            var result = await service.DisburseLoanAsync(loanId: 1);

            Assert.That(result, Is.False);
        }

        // ─────────────────────────────────────────────────────
        // LOAN PRODUCTS TEST
        // ─────────────────────────────────────────────────────

        [Test]
        public async Task GetLoanProducts_ShouldReturn_OnlyActiveProducts()
        {
            using var db = GetDb("Loan_Products");
            db.LoanProducts.AddRange(
                new LoanProduct { LoanProductId = 1, ProductName = "Home Loan", LoanAmount = 500000, InterestRate = 8.5m, TenureMonths = 120, IsActive = true },
                new LoanProduct { LoanProductId = 2, ProductName = "Car Loan", LoanAmount = 100000, InterestRate = 9.0m, TenureMonths = 60, IsActive = true },
                new LoanProduct { LoanProductId = 3, ProductName = "Old Scheme", LoanAmount = 50000, InterestRate = 15m, TenureMonths = 12, IsActive = false }
            );
            await db.SaveChangesAsync();

            var service = new LoanService(db);
            var result = await service.GetLoanProductsAsync();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.All(p => p.LoanProductId != 3), Is.True);
        }
    }
}