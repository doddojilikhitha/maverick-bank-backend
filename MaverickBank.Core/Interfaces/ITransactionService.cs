using MaverickBank.Core.DTOs;

namespace MaverickBank.Core.Interfaces
{
    public interface ITransactionService
    {
        Task<(bool Success, string Message)> DepositAsync(DepositWithdrawDTO dto);
        Task<(bool Success, string Message)> WithdrawAsync(DepositWithdrawDTO dto);
        Task<(bool Success, string Message)> TransferAsync(TransferDTO dto);
        Task<List<TransactionResponseDTO>> GetLast10Async(int accountId);
        Task<List<TransactionResponseDTO>> GetLastMonthAsync(int accountId);
        Task<List<TransactionResponseDTO>> GetBetweenDatesAsync(int accountId, DateTime from, DateTime to);
        Task<List<TransactionResponseDTO>> GetAllTransactionsAsync(
        int pageNumber = 1,
        int pageSize = 10,
        string? sortBy = null,
        string? type = null);
        Task<TransactionSummaryDTO> GetAccountSummaryAsync(int accountId);
    }
}
