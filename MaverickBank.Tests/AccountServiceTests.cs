using MaverickBank.Core.DTOs;
using MaverickBank.Core.Entities;
using MaverickBank.Infrastructure.Data;
using MaverickBank.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace MaverickBank.Tests
{
    [TestFixture]
    public class AccountServiceTests
    {
        private AppDbContext GetDb(string name) =>
            new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(name).Options);

        [Test]
        public async Task OpenAccount_ShouldSucceed_WhenUserExists()
        {
            using var db = GetDb("Acc_Open_Success");
            db.Users.Add(new User { UserId = 1, FullName = "Alice", Email = "a@t.com", PasswordHash = "h", Role = "Customer" });
            await db.SaveChangesAsync();

            var service = new AccountService(db);
            var result = await service.OpenAccountAsync(new OpenAccountDTO { UserId = 1, AccountType = "Savings" });

            Assert.That(result, Is.True);
            Assert.That((await db.Accounts.FirstAsync()).Status, Is.EqualTo("Pending"));
        }

        [Test]
        public async Task OpenAccount_ShouldFail_WhenUserNotFound()
        {
            using var db = GetDb("Acc_Open_NoUser");
            var service = new AccountService(db);
            var result = await service.OpenAccountAsync(new OpenAccountDTO { UserId = 99, AccountType = "Savings" });
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ApproveAccount_ShouldSetStatusActive()
        {
            using var db = GetDb("Acc_Approve");
            db.Accounts.Add(new Account { AccountId = 1, UserId = 1, AccountNumber = "ACC001", Status = "Pending" });
            await db.SaveChangesAsync();

            var service = new AccountService(db);
            var result = await service.ApproveAccountAsync(1);

            Assert.That(result, Is.True);
            Assert.That((await db.Accounts.FindAsync(1))!.Status, Is.EqualTo("Active"));
        }

        [Test]
        public async Task ApproveAccount_ShouldFail_WhenNotPending()
        {
            using var db = GetDb("Acc_Approve_NotPending");
            db.Accounts.Add(new Account { AccountId = 1, UserId = 1, AccountNumber = "ACC001", Status = "Active" });
            await db.SaveChangesAsync();

            var service = new AccountService(db);
            var result = await service.ApproveAccountAsync(1);
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task CloseAccount_ShouldSetStatusClosed()
        {
            using var db = GetDb("Acc_Close");
            db.Accounts.Add(new Account { AccountId = 1, UserId = 1, AccountNumber = "ACC001", Status = "Active" });
            await db.SaveChangesAsync();

            var service = new AccountService(db);
            var result = await service.CloseAccountAsync(1);

            Assert.That(result, Is.True);
            Assert.That((await db.Accounts.FindAsync(1))!.Status, Is.EqualTo("Closed"));
        }

        [Test]
        public async Task RequestClose_ShouldSetStatusCloseRequested()
        {
            using var db = GetDb("Acc_CloseReq");
            db.Accounts.Add(new Account { AccountId = 1, UserId = 1, AccountNumber = "ACC001", Status = "Active" });
            await db.SaveChangesAsync();

            var service = new AccountService(db);
            var result = await service.RequestCloseAccountAsync(1);

            Assert.That(result, Is.True);
            Assert.That((await db.Accounts.FindAsync(1))!.Status, Is.EqualTo("CloseRequested"));
        }

        [Test]
        public async Task AddBeneficiary_ShouldSucceed_WhenNotDuplicate()
        {
            using var db = GetDb("Acc_Beneficiary_Add");
            var service = new AccountService(db);

            var result = await service.AddBeneficiaryAsync(new BeneficiaryDTO
            {
                UserId = 1,
                AccountName = "Bob",
                AccountNumber = "ACC999",
                IFSCCode = "MAVK0001"
            });

            Assert.That(result, Is.True);
            Assert.That(await db.Beneficiaries.CountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task AddBeneficiary_ShouldFail_WhenDuplicate()
        {
            using var db = GetDb("Acc_Beneficiary_Dup");
            db.Beneficiaries.Add(new Beneficiary { UserId = 1, AccountName = "Bob", AccountNumber = "ACC999" });
            await db.SaveChangesAsync();

            var service = new AccountService(db);
            var result = await service.AddBeneficiaryAsync(new BeneficiaryDTO
            {
                UserId = 1,
                AccountName = "Bob",
                AccountNumber = "ACC999"
            });

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task GetMyAccounts_ShouldReturn_OnlyActiveAccounts()
        {
            using var db = GetDb("Acc_GetMy");
            db.Users.Add(new User { UserId = 1, FullName = "Alice", Email = "a@t.com", PasswordHash = "h", Role = "Customer" });
            db.Accounts.AddRange(
                new Account { AccountId = 1, UserId = 1, AccountNumber = "ACC001", Status = "Active" },
                new Account { AccountId = 2, UserId = 1, AccountNumber = "ACC002", Status = "Closed" },
                new Account { AccountId = 3, UserId = 1, AccountNumber = "ACC003", Status = "Pending" }
            );
            await db.SaveChangesAsync();

            var service = new AccountService(db);
            var result = await service.GetMyAccountsAsync(1);

            Assert.That(result.Count, Is.EqualTo(2));
        }
    }
}