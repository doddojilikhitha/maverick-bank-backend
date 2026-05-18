using MaverickBank.Core.DTOs;
using MaverickBank.Core.Entities;
using MaverickBank.Infrastructure.Data;
using MaverickBank.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace MaverickBank.Tests
{
    [TestFixture]
    public class TransactionServiceTests
    {
        private AppDbContext GetDb(string name) =>
            new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(name).Options);

        [Test]
        public async Task Deposit_ShouldIncreaseBalance()
        {
            using var db = GetDb("Txn_Deposit");
            db.Accounts.Add(new Account
            {
                AccountId = 1,
                UserId = 1,
                AccountNumber = "ACC001",
                Balance = 1000,
                Status = "Active"
            });
            await db.SaveChangesAsync();

            var service = new TransactionService(db);
            var (success, msg) = await service.DepositAsync(
                new DepositWithdrawDTO { AccountId = 1, Amount = 500 });

            Assert.That(success, Is.True);
            Assert.That((await db.Accounts.FindAsync(1))!.Balance, Is.EqualTo(1500));
        }

        [Test]
        public async Task Deposit_ShouldFail_WhenAmountIsNegative()
        {
            using var db = GetDb("Txn_Deposit_Neg");
            db.Accounts.Add(new Account
            {
                AccountId = 1,
                UserId = 1,
                AccountNumber = "ACC001",
                Balance = 1000,
                Status = "Active"
            });
            await db.SaveChangesAsync();

            var service = new TransactionService(db);
            var (success, msg) = await service.DepositAsync(
                new DepositWithdrawDTO { AccountId = 1, Amount = -500 });

            Assert.That(success, Is.False);
            Assert.That(msg, Does.Contain("greater than zero"));
        }

        [Test]
        public async Task Deposit_ShouldFail_WhenAccountInactive()
        {
            using var db = GetDb("Txn_Deposit_Inactive");
            db.Accounts.Add(new Account
            {
                AccountId = 1,
                UserId = 1,
                AccountNumber = "ACC001",
                Balance = 1000,
                Status = "Closed"
            });
            await db.SaveChangesAsync();

            var service = new TransactionService(db);
            var (success, msg) = await service.DepositAsync(
                new DepositWithdrawDTO { AccountId = 1, Amount = 500 });

            Assert.That(success, Is.False);
            Assert.That(msg, Does.Contain("inactive"));
        }

        [Test]
        public async Task Withdraw_ShouldDecreaseBalance()
        {
            using var db = GetDb("Txn_Withdraw");
            db.Accounts.Add(new Account
            {
                AccountId = 1,
                UserId = 1,
                AccountNumber = "ACC001",
                Balance = 5000,
                Status = "Active"
            });
            await db.SaveChangesAsync();

            var service = new TransactionService(db);
            var (success, msg) = await service.WithdrawAsync(
                new DepositWithdrawDTO { AccountId = 1, Amount = 1000 });

            Assert.That(success, Is.True);
            Assert.That((await db.Accounts.FindAsync(1))!.Balance, Is.EqualTo(4000));
        }

        [Test]
        public async Task Withdraw_ShouldFail_WhenInsufficientBalance()
        {
            using var db = GetDb("Txn_Withdraw_Insuf");
            db.Accounts.Add(new Account
            {
                AccountId = 1,
                UserId = 1,
                AccountNumber = "ACC001",
                Balance = 100,
                Status = "Active"
            });
            await db.SaveChangesAsync();

            var service = new TransactionService(db);
            var (success, msg) = await service.WithdrawAsync(
                new DepositWithdrawDTO { AccountId = 1, Amount = 500 });

            Assert.That(success, Is.False);
            Assert.That(msg, Does.Contain("Insufficient"));
        }

        [Test]
        public async Task Withdraw_ShouldFail_WhenBelowMinimumBalance()
        {
            using var db = GetDb("Txn_Withdraw_MinBal");
            db.Accounts.Add(new Account
            {
                AccountId = 1,
                UserId = 1,
                AccountNumber = "ACC001",
                Balance = 1000,
                Status = "Active"
            });
            await db.SaveChangesAsync();

            var service = new TransactionService(db);
            var (success, msg) = await service.WithdrawAsync(
                new DepositWithdrawDTO { AccountId = 1, Amount = 600 });

            Assert.That(success, Is.False);
            Assert.That(msg, Does.Contain("Minimum balance"));
        }

        [Test]
        public async Task Transfer_ShouldMoveFunds_BetweenAccounts()
        {
            using var db = GetDb("Txn_Transfer");
            db.Accounts.AddRange(
                new Account
                {
                    AccountId = 1,
                    UserId = 1,
                    AccountNumber = "ACC001",
                    Balance = 5000,
                    Status = "Active",
                    IFSCCode = "MAVK0001"
                },
                new Account
                {
                    AccountId = 2,
                    UserId = 2,
                    AccountNumber = "ACC002",
                    Balance = 500,
                    Status = "Active",
                    IFSCCode = "MAVK0001"
                }
            );
            await db.SaveChangesAsync();

            var service = new TransactionService(db);
            var (success, msg) = await service.TransferAsync(new TransferDTO
            {
                FromAccountId = 1,
                ToAccountId = 2,
                UserId = 1,
                Amount = 1000
            });

            Assert.That(success, Is.True);
            Assert.That((await db.Accounts.FindAsync(1))!.Balance, Is.EqualTo(4000));
            Assert.That((await db.Accounts.FindAsync(2))!.Balance, Is.EqualTo(1500));
        }

        [Test]
        public async Task Transfer_ShouldFail_WhenSameAccount()
        {
            using var db = GetDb("Txn_Transfer_Same");
            db.Accounts.Add(new Account
            {
                AccountId = 1,
                UserId = 1,
                AccountNumber = "ACC001",
                Balance = 5000,
                Status = "Active"
            });
            await db.SaveChangesAsync();

            var service = new TransactionService(db);
            var (success, msg) = await service.TransferAsync(new TransferDTO
            {
                FromAccountId = 1,
                ToAccountId = 1,
                UserId = 1,
                Amount = 1000
            });

            Assert.That(success, Is.False);
            Assert.That(msg, Does.Contain("same account"));
        }

        [Test]
        public async Task Transfer_ShouldFail_WhenAccountInactive()
        {
            using var db = GetDb("Txn_Transfer_Inactive");
            db.Accounts.AddRange(
                new Account
                {
                    AccountId = 1,
                    UserId = 1,
                    AccountNumber = "ACC001",
                    Balance = 5000,
                    Status = "Closed",
                    IFSCCode = "MAVK0001"
                },
                new Account
                {
                    AccountId = 2,
                    UserId = 2,
                    AccountNumber = "ACC002",
                    Balance = 0,
                    Status = "Active",
                    IFSCCode = "MAVK0001"
                }
            );
            await db.SaveChangesAsync();

            var service = new TransactionService(db);
            var (success, msg) = await service.TransferAsync(new TransferDTO
            {
                FromAccountId = 1,
                ToAccountId = 2,
                UserId = 1,
                Amount = 1000
            });

            Assert.That(success, Is.False);
            Assert.That(msg, Does.Contain("inactive"));
        }

        [Test]
        public async Task Transfer_ShouldFail_WhenInsufficientBalance()
        {
            using var db = GetDb("Txn_Transfer_Insuf");
            db.Accounts.AddRange(
                new Account
                {
                    AccountId = 1,
                    UserId = 1,
                    AccountNumber = "ACC001",
                    Balance = 100,
                    Status = "Active",
                    IFSCCode = "MAVK0001"
                },
                new Account
                {
                    AccountId = 2,
                    UserId = 2,
                    AccountNumber = "ACC002",
                    Balance = 0,
                    Status = "Active",
                    IFSCCode = "MAVK0001"
                }
            );
            await db.SaveChangesAsync();

            var service = new TransactionService(db);
            var (success, msg) = await service.TransferAsync(new TransferDTO
            {
                FromAccountId = 1,
                ToAccountId = 2,
                UserId = 1,
                Amount = 500
            });

            Assert.That(success, Is.False);
            Assert.That(msg, Does.Contain("Insufficient"));
        }

        [Test]
        public async Task GetLast10_ShouldReturn_MaxTenRecords()
        {
            using var db = GetDb("Txn_Last10");
            for (int i = 1; i <= 15; i++)
                db.Transactions.Add(new Transaction
                { AccountId = 1, Type = "Deposit", Amount = i * 100 });
            await db.SaveChangesAsync();

            var service = new TransactionService(db);
            var result = await service.GetLast10Async(1);

            Assert.That(result.Count, Is.EqualTo(10));
        }

        [Test]
        public async Task GetAccountSummary_ShouldCalculate_InboundOutbound()
        {
            using var db = GetDb("Txn_Summary");
            db.Transactions.AddRange(
                new Transaction { AccountId = 1, Type = "Deposit", Amount = 5000 },
                new Transaction { AccountId = 1, Type = "Deposit", Amount = 3000 },
                new Transaction { AccountId = 1, Type = "Withdrawal", Amount = 2000 },
                new Transaction { AccountId = 1, Type = "Transfer", Amount = 1000 }
            );
            await db.SaveChangesAsync();

            var service = new TransactionService(db);
            var result = await service.GetAccountSummaryAsync(1);

            Assert.That(result.TotalInbound, Is.EqualTo(8000));
            Assert.That(result.TotalOutbound, Is.EqualTo(3000));
        }
    }
}