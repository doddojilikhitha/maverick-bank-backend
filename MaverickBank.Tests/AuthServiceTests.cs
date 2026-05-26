using MaverickBank.Core.DTOs;
using MaverickBank.Core.Entities;
using MaverickBank.Infrastructure.Data;
using MaverickBank.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace MaverickBank.Tests
{
    [TestFixture]
    public class AuthServiceTests
    {
        private AppDbContext GetDb(string name) =>
            new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(name).Options);

        private IConfiguration GetConfig()
        {
            var dict = new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "TestSecretKeyMustBe32CharsLong!!",
                ["JwtSettings:Issuer"] = "MaverickBank",
                ["JwtSettings:Audience"] = "MaverickBankUsers",
                ["JwtSettings:ExpiryMinutes"] = "60"
            };
            return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
        }

        [Test]
        public async Task Register_ShouldSucceed_WithNewEmail()
        {
            using var db = GetDb("Auth_Register_Success");
            var service = new AuthService(db, GetConfig());

            var result = await service.RegisterAsync(new RegisterDTO
            {
                FullName = "Alice",
                Email = "alice@test.com",
                Password = "Pass@1234"
            });

            Assert.That(result, Is.True);
            Assert.That(await db.Users.CountAsync(), Is.EqualTo(1));
        }

        [Test]
        public async Task Register_ShouldFail_WhenEmailAlreadyExists()
        {
            using var db = GetDb("Auth_Register_Duplicate");
            var service = new AuthService(db, GetConfig());

            await service.RegisterAsync(new RegisterDTO { FullName = "Alice", Email = "alice@test.com", Password = "Pass@1234" });
            var result = await service.RegisterAsync(new RegisterDTO { FullName = "Alice2", Email = "alice@test.com", Password = "Pass@5678" });

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task Login_ShouldReturnToken_WithValidCredentials()
        {
            using var db = GetDb("Auth_Login_Success");
            var service = new AuthService(db, GetConfig());

            await service.RegisterAsync(new RegisterDTO { FullName = "Bob", Email = "bob@test.com", Password = "Pass@1234" });
            var result = await service.LoginAsync(new LoginDTO { Email = "bob@test.com", Password = "Pass@1234" });

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Token, Is.Not.Empty);
            Assert.That(result.Role, Is.EqualTo("Customer"));
        }

        [Test]
        public async Task Login_ShouldFail_WithWrongPassword()
        {
            using var db = GetDb("Auth_Login_WrongPwd");
            var service = new AuthService(db, GetConfig());

            await service.RegisterAsync(new RegisterDTO { FullName = "Bob", Email = "bob@test.com", Password = "Pass@1234" });
            var result = await service.LoginAsync(new LoginDTO { Email = "bob@test.com", Password = "WrongPass" });

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task Login_ShouldFail_WhenUserIsInactive()
        {
            using var db = GetDb("Auth_Login_Inactive");
            var service = new AuthService(db, GetConfig());

            await service.RegisterAsync(new RegisterDTO { FullName = "Carol", Email = "carol@test.com", Password = "Pass@1234" });
            var user = await db.Users.FirstAsync();
            user.IsActive = false;
            await db.SaveChangesAsync();

            var result = await service.LoginAsync(new LoginDTO { Email = "carol@test.com", Password = "Pass@1234" });

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task Register_ShouldHashPassword()
        {
            using var db = GetDb("Auth_Register_Hash");
            var service = new AuthService(db, GetConfig());

            await service.RegisterAsync(new RegisterDTO { FullName = "Dave", Email = "dave@test.com", Password = "Pass@1234" });
            var user = await db.Users.FirstAsync();

            Assert.That(user.PasswordHash, Is.Not.EqualTo("Pass@1234"));
            Assert.That(BCrypt.Net.BCrypt.Verify("Pass@1234", user.PasswordHash), Is.True);
        }
        [Test]
        public async Task Register_ShouldFail_WhenAgeBelow18()
        {
            using var db = GetDb("Auth_Register_Underage");
            var service = new AuthService(db, GetConfig());

            var result = await service.RegisterAsync(new RegisterDTO
            {
                FullName = "Young User",
                Email = "young@test.com",
                Password = "Pass@1234",
                DOB = DateTime.Today.AddYears(-16) // 16 years old
            });

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task Register_ShouldSucceed_WhenAge18OrAbove()
        {
            using var db = GetDb("Auth_Register_ValidAge");
            var service = new AuthService(db, GetConfig());

            var result = await service.RegisterAsync(new RegisterDTO
            {
                FullName = "Adult User",
                Email = "adult@test.com",
                Password = "Pass@1234",
                DOB = DateTime.Today.AddYears(-20) // 20 years old
            });

            Assert.That(result, Is.True);
        }
    }
}