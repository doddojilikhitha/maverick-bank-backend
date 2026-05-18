using MaverickBank.Core.DTOs;

namespace MaverickBank.Core.Interfaces
{
    public interface IReportService
    {
        Task<List<TransactionResponseDTO>> GetAccountStatementAsync(int accountId);
        Task<TransactionSummaryDTO> GetFinancialPerformanceAsync();
    }
}