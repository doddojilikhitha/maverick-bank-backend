using MaverickBank.Core.DTOs;
using MaverickBank.Core.Entities;
using MaverickBank.Core.Interfaces;
using MaverickBank.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaverickBank.Infrastructure.Services
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _db;
        public AccountService(AppDbContext db) => _db = db;

        public async Task<List<AccountResponseDTO>> GetMyAccountsAsync(int userId) =>
            await _db.Accounts
                .Where(a => a.UserId == userId && a.Status != "Closed")
                .Include(a => a.User)
                .Select(a => MapToDTO(a))
                .ToListAsync();

        public async Task<AccountResponseDTO?> GetAccountByIdAsync(int accountId)
        {
            var a = await _db.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AccountId == accountId);
            return a == null ? null : MapToDTO(a);
        }

        public async Task<List<AccountResponseDTO>> GetAllAccountsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? status = null,
            string? sortBy = null)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var query = _db.Accounts
                .Include(a => a.User)
                .AsQueryable();

            //Filtering
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.Status.ToLower() == status.ToLower());
            }

            //Sorting
            query = sortBy?.ToLower() switch
            {
                "balance" => query.OrderByDescending(a => a.Balance),
                "date" => query.OrderByDescending(a => a.CreatedAt),
                _ => query.OrderByDescending(a => a.CreatedAt)
            };

            //Pagination
            query = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return await query
                .Select(a => MapToDTO(a))
                .ToListAsync();
        }

        public async Task<bool> OpenAccountAsync(OpenAccountDTO dto)
        {
            var user = await _db.Users.FindAsync(dto.UserId);
            if (user == null) return false;

            _db.Accounts.Add(new Account
            {
                AccountNumber = GenerateAccountNumber(),
                UserId = dto.UserId,
                AccountType = dto.AccountType,
                BranchName = dto.BranchName,
                IFSCCode = dto.IFSCCode,
                BranchAddress = dto.BranchAddress,
                Status = "Pending",
                Balance = 0
            });

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RequestCloseAccountAsync(int accountId)
        {
            var account = await _db.Accounts.FindAsync(accountId);
            if (account == null || account.Status != "Active") return false;
            account.Status = "CloseRequested";
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ApproveAccountAsync(int accountId)
        {
            var account = await _db.Accounts.FindAsync(accountId);
            if (account == null || account.Status != "Pending") return false;
            account.Status = "Active";
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CloseAccountAsync(int accountId)
        {
            var account = await _db.Accounts.FindAsync(accountId);
            if (account == null) return false;
            account.Status = "Closed";
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddBeneficiaryAsync(BeneficiaryDTO dto)
        {
            var exists = await _db.Beneficiaries.AnyAsync(b =>
                b.UserId == dto.UserId && b.AccountNumber == dto.AccountNumber);
            if (exists) return false;

            _db.Beneficiaries.Add(new Beneficiary
            {
                UserId = dto.UserId,
                AccountName = dto.AccountName,
                AccountNumber = dto.AccountNumber,
                BankName = dto.BankName,
                BranchName = dto.BranchName,
                IFSCCode = dto.IFSCCode
            });

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<BeneficiaryDTO>> GetBeneficiariesAsync(int userId) =>
            await _db.Beneficiaries
                .Where(b => b.UserId == userId)
                .Select(b => new BeneficiaryDTO
                {
                    UserId = b.UserId,
                    AccountName = b.AccountName,
                    AccountNumber = b.AccountNumber,
                    BankName = b.BankName,
                    BranchName = b.BranchName,
                    IFSCCode = b.IFSCCode
                }).ToListAsync();

        private static string GenerateAccountNumber() =>
            "MAV" + DateTime.UtcNow.Ticks.ToString()[10..];

        private static AccountResponseDTO MapToDTO(Account a) => new()
        {
            AccountId = a.AccountId,
            AccountNumber = a.AccountNumber,
            AccountType = a.AccountType,
            Balance = a.Balance,
            IFSCCode = a.IFSCCode,
            BranchName = a.BranchName,
            Status = a.Status,
            OwnerName = a.User?.FullName ?? string.Empty,
            CreatedAt = a.CreatedAt
        };
    }
}