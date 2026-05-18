using MaverickBank.Core.DTOs;
using MaverickBank.Core.Entities;
using MaverickBank.Core.Interfaces;
using MaverickBank.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaverickBank.Infrastructure.Services
{
    public class LoanService : ILoanService
    {
        private readonly AppDbContext _db;
        public LoanService(AppDbContext db) => _db = db;

        public async Task<List<LoanProductResponseDTO>> GetLoanProductsAsync() =>
            await _db.LoanProducts
                .Where(lp => lp.IsActive)
                .Select(lp => new LoanProductResponseDTO
                {
                    LoanProductId = lp.LoanProductId,
                    ProductName = lp.ProductName,
                    LoanAmount = lp.LoanAmount,
                    InterestRate = lp.InterestRate,
                    TenureMonths = lp.TenureMonths
                }).ToListAsync();

        public async Task<bool> ApplyLoanAsync(LoanApplyDTO dto)
        {
            var account = await _db.Accounts.FindAsync(dto.AccountId);
            if (account == null || account.Status != "Active") return false;

            var product = await _db.LoanProducts.FindAsync(dto.LoanProductId);
            if (product == null || !product.IsActive) return false;

            _db.Loans.Add(new Loan
            {
                UserId = dto.UserId,
                LoanProductId = dto.LoanProductId,
                AccountId = dto.AccountId,
                AmountApplied = dto.AmountApplied,
                Purpose = dto.Purpose,
                Status = "Pending"
            });

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<LoanResponseDTO>> GetMyLoansAsync(int userId) =>
            await _db.Loans
                .Where(l => l.UserId == userId)
                .Include(l => l.LoanProduct)
                .Select(l => MapToDTO(l))
                .ToListAsync();

        public async Task<List<LoanResponseDTO>> GetAllLoansAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? status = null,
            string? sortBy = null)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var query = _db.Loans
                .Include(l => l.LoanProduct)
                .AsQueryable();

            //Filtering
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(l => l.Status.ToLower() == status.ToLower());
            }

            //Sorting
            query = sortBy?.ToLower() switch
            {
                "amount" => query.OrderByDescending(l => l.AmountApplied),
                "date" => query.OrderByDescending(l => l.AppliedOn),
                _ => query.OrderByDescending(l => l.AppliedOn)
            };

            //Pagination
            query = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            return await query
                .Select(l => new LoanResponseDTO
                {
                    LoanId = l.LoanId,
                    UserId = l.UserId,
                    ProductName = l.LoanProduct.ProductName,
                    AmountApplied = l.AmountApplied,
                    InterestRate = l.LoanProduct.InterestRate,
                    TenureMonths = l.LoanProduct.TenureMonths,
                    Purpose = l.Purpose,
                    Status = l.Status,
                    AppliedOn = l.AppliedOn,
                    DisbursedOn = l.DisbursedOn
                })
                .ToListAsync();
        }

        public async Task<bool> ApproveLoanAsync(int loanId, int reviewedByEmployeeId)
        {
            var loan = await _db.Loans
                .Include(l => l.Account)
                .FirstOrDefaultAsync(l => l.LoanId == loanId);

            if (loan == null || loan.Status != "Pending") return false;

            // Creditworthiness Check
            var totalInbound = await _db.Transactions
                .Where(t => t.AccountId == loan.AccountId && t.Type == "Deposit")
                .SumAsync(t => t.Amount);

            var totalOutbound = await _db.Transactions
                .Where(t => t.AccountId == loan.AccountId
                         && (t.Type == "Withdrawal" || t.Type == "Transfer"))
                .SumAsync(t => t.Amount);

            var currentBalance = loan.Account.Balance;
            var netCashFlow = totalInbound - totalOutbound;

            // Rule 1: Balance > 5000
            // Rule 2: Total inbound > 10% of loan amount
            // Rule 3: Net cash flow must be positive
            bool isCreditWorthy =
                currentBalance > 5000 &&
                totalInbound > (loan.AmountApplied * 0.1m) &&
                netCashFlow > 0;

            if (!isCreditWorthy)
            {
                loan.Status = "Rejected";
                loan.ReviewedBy = reviewedByEmployeeId;
                loan.ReviewedOn = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return false;
            }

            loan.Status = "Approved";
            loan.ReviewedBy = reviewedByEmployeeId;
            loan.ReviewedOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectLoanAsync(int loanId, int reviewedByEmployeeId)
        {
            var loan = await _db.Loans.FindAsync(loanId);
            if (loan == null || loan.Status != "Pending") return false;

            loan.Status = "Rejected";
            loan.ReviewedBy = reviewedByEmployeeId;
            loan.ReviewedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DisburseLoanAsync(int loanId)
        {
            var loan = await _db.Loans
                .Include(l => l.Account)
                .FirstOrDefaultAsync(l => l.LoanId == loanId);

            if (loan == null || loan.Status != "Approved") return false;

            loan.Account.Balance += loan.AmountApplied;
            loan.Status = "Disbursed";
            loan.DisbursedOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        private static LoanResponseDTO MapToDTO(Loan l) => new()
        {
            LoanId = l.LoanId,
            UserId = l.UserId,
            ProductName = l.LoanProduct?.ProductName ?? string.Empty,
            AmountApplied = l.AmountApplied,
            InterestRate = l.LoanProduct?.InterestRate ?? 0,
            TenureMonths = l.LoanProduct?.TenureMonths ?? 0,
            Purpose = l.Purpose,
            Status = l.Status,
            AppliedOn = l.AppliedOn,
            DisbursedOn = l.DisbursedOn
        };
    }
}