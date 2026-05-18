using MaverickBank.Core.DTOs;
using MaverickBank.Core.Interfaces;
using MaverickBank.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MaverickBank.Infrastructure.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _db;
        public ReportService(AppDbContext db) => _db = db;

        public async Task<List<TransactionResponseDTO>> GetAccountStatementAsync(int accountId) =>
            await _db.Transactions
                .Where(t => t.AccountId == accountId)
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => new TransactionResponseDTO
                {
                    TransactionId = t.TransactionId,
                    Type = t.Type,
                    Amount = t.Amount,
                    Description = t.Description,
                    TransactionDate = t.TransactionDate,
                    AccountId = t.AccountId,
                    ToAccountId = t.ToAccountId
                }).ToListAsync();

        public async Task<TransactionSummaryDTO> GetFinancialPerformanceAsync()
        {
            var all = await _db.Transactions.ToListAsync();
            return new TransactionSummaryDTO
            {
                TotalInbound = all.Where(t => t.Type == "Deposit").Sum(t => t.Amount),
                TotalOutbound = all.Where(t => t.Type is "Withdrawal" or "Transfer").Sum(t => t.Amount),
                Transactions = all.Select(t => new TransactionResponseDTO
                {
                    TransactionId = t.TransactionId,
                    Type = t.Type,
                    Amount = t.Amount,
                    Description = t.Description,
                    TransactionDate = t.TransactionDate,
                    AccountId = t.AccountId,
                    ToAccountId = t.ToAccountId
                }).ToList()
            };
        }
    }
}