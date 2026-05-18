using MaverickBank.Core.DTOs;

namespace MaverickBank.Core.Interfaces
{
    public interface ILoanService
    {
        Task<List<LoanProductResponseDTO>> GetLoanProductsAsync();
        Task<bool> ApplyLoanAsync(LoanApplyDTO dto);
        Task<List<LoanResponseDTO>> GetMyLoansAsync(int userId);
        Task<List<LoanResponseDTO>> GetAllLoansAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? status = null,
            string? sortBy = null);
        Task<bool> ApproveLoanAsync(int loanId, int reviewedByEmployeeId);
        Task<bool> RejectLoanAsync(int loanId, int reviewedByEmployeeId);
        Task<bool> DisburseLoanAsync(int loanId);
    }
}
