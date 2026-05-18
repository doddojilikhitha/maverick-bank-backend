using MaverickBank.Core.DTOs;
using MaverickBank.Core.Entities;
using MaverickBank.Core.Interfaces;
using MaverickBank.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaverickBank.Infrastructure.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly AppDbContext _db;
        public TransactionService(AppDbContext db) => _db = db;

        public async Task<(bool Success, string Message)> DepositAsync(DepositWithdrawDTO dto)
        {
            if (dto.Amount <= 0)
                return (false, "Deposit amount must be greater than zero.");

            var account = await _db.Accounts.FindAsync(dto.AccountId);
            if (account == null)
                return (false, "Account not found.");
            if (account.Status != "Active")
                return (false, "Cannot deposit to an inactive or closed account.");

            account.Balance += dto.Amount;
            _db.Transactions.Add(new Transaction
            {
                AccountId = dto.AccountId,
                Type = "Deposit",
                Amount = dto.Amount,
                Description = dto.Description
            });

            await _db.SaveChangesAsync();
            return (true, "Deposit successful.");
        }

        public async Task<(bool Success, string Message)> WithdrawAsync(DepositWithdrawDTO dto)
        {
            if (dto.Amount <= 0)
                return (false, "Withdrawal amount must be greater than zero.");

            var account = await _db.Accounts.FindAsync(dto.AccountId);
            if (account == null)
                return (false, "Account not found.");
            if (account.Status != "Active")
                return (false, "Cannot withdraw from an inactive or closed account.");
            if (account.Balance < dto.Amount)
                return (false, $"Insufficient balance. Available: {account.Balance}");
            if ((account.Balance - dto.Amount) < 500)
                return (false, "Minimum balance of 500 must be maintained.");

            account.Balance -= dto.Amount;
            _db.Transactions.Add(new Transaction
            {
                AccountId = dto.AccountId,
                Type = "Withdrawal",
                Amount = dto.Amount,
                Description = dto.Description
            });

            await _db.SaveChangesAsync();
            return (true, "Withdrawal successful.");
        }

        public async Task<(bool Success, string Message)> TransferAsync(TransferDTO dto)
        {
            if (dto.Amount <= 0)
                return (false, "Transfer amount must be greater than zero.");
            if (dto.FromAccountId == dto.ToAccountId)
                return (false, "Cannot transfer to the same account.");

            var from = await _db.Accounts.FindAsync(dto.FromAccountId);
            var to = await _db.Accounts.FindAsync(dto.ToAccountId);

            if (from == null) return (false, "Source account not found.");
            if (to == null) return (false, "Destination account not found.");
            if (from.Status != "Active") return (false, "Source account is inactive or closed.");
            if (to.Status != "Active") return (false, "Destination account is inactive or closed.");
            if (from.Balance < dto.Amount)
                return (false, $"Insufficient balance. Available: {from.Balance}");
            if ((from.Balance - dto.Amount) < 500)
                return (false, "Minimum balance of 500 must be maintained after transfer.");

            try
            {
                from.Balance -= dto.Amount;
                to.Balance += dto.Amount;

                _db.Transactions.Add(new Transaction
                {
                    AccountId = dto.FromAccountId,
                    ToAccountId = dto.ToAccountId,
                    Type = "Transfer",
                    Amount = dto.Amount,
                    Description = dto.Description
                });

                var exists = await _db.Beneficiaries.AnyAsync(b =>
                    b.UserId == dto.UserId && b.AccountNumber == to.AccountNumber);
                if (!exists)
                    _db.Beneficiaries.Add(new Beneficiary
                    {
                        UserId = dto.UserId,
                        AccountNumber = to.AccountNumber,
                        AccountName = to.AccountNumber,
                        IFSCCode = to.IFSCCode
                    });

                await _db.SaveChangesAsync();
                return (true, "Transfer successful.");
            }
            catch (Exception ex)
            {
                return (false, $"Transfer failed: {ex.Message}");
            }
        }

        public async Task<List<TransactionResponseDTO>> GetLast10Async(int accountId) =>
            await _db.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.TransactionDate)
                .Take(10)
                .Select(t => MapToDTO(t))
                .ToListAsync();

        public async Task<List<TransactionResponseDTO>> GetLastMonthAsync(int accountId)
        {
            var from = DateTime.UtcNow.AddMonths(-1);
            return await _db.Transactions
                .Where(t => t.AccountId == accountId && t.TransactionDate >= from)
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => MapToDTO(t))
                .ToListAsync();
        }

        public async Task<List<TransactionResponseDTO>> GetBetweenDatesAsync(
            int accountId, DateTime from, DateTime to) =>
            await _db.Transactions
                .Where(t => t.AccountId == accountId
                         && t.TransactionDate >= from
                         && t.TransactionDate <= to)
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => MapToDTO(t))
                .ToListAsync();

        public async Task<List<TransactionResponseDTO>> GetAllTransactionsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? sortBy = null,
            string? type = null)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var query = _db.Transactions.AsQueryable();

            //Filtering
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(t => t.Type.ToLower() == type.ToLower());
            }

            //Sorting
            query = sortBy?.ToLower() switch
            {
                "amount" => query.OrderByDescending(t => t.Amount),
                "date" => query.OrderByDescending(t => t.TransactionDate),
                _ => query.OrderByDescending(t => t.TransactionDate)
            };

            //Pagination
            query = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return await query
                .Select(t => MapToDTO(t))
                .ToListAsync();
        }

        public async Task<TransactionSummaryDTO> GetAccountSummaryAsync(int accountId)
        {
            var txns = await _db.Transactions
                .Where(t => t.AccountId == accountId)
                .ToListAsync();

            return new TransactionSummaryDTO
            {
                TotalInbound = txns.Where(t => t.Type == "Deposit").Sum(t => t.Amount),
                TotalOutbound = txns.Where(t => t.Type is "Withdrawal" or "Transfer").Sum(t => t.Amount),
                Transactions = txns.Select(t => MapToDTO(t)).ToList()
            };
        }

        private static TransactionResponseDTO MapToDTO(Transaction t) => new()
        {
            TransactionId = t.TransactionId,
            Type = t.Type,
            Amount = t.Amount,
            Description = t.Description,
            TransactionDate = t.TransactionDate,
            AccountId = t.AccountId,
            ToAccountId = t.ToAccountId
        };
    }
}