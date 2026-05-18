using MaverickBank.Core.DTOs;

namespace MaverickBank.Core.Interfaces
{
    public interface IAccountService
    {
        Task<List<AccountResponseDTO>> GetMyAccountsAsync(int userId);
        Task<AccountResponseDTO?> GetAccountByIdAsync(int accountId);
        Task<List<AccountResponseDTO>> GetAllAccountsAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string? status = null,
            string? sortBy = null);
        Task<bool> OpenAccountAsync(OpenAccountDTO dto);
        Task<bool> RequestCloseAccountAsync(int accountId);
        Task<bool> ApproveAccountAsync(int accountId);
        Task<bool> CloseAccountAsync(int accountId);
        Task<bool> AddBeneficiaryAsync(BeneficiaryDTO dto);
        Task<List<BeneficiaryDTO>> GetBeneficiariesAsync(int userId);
    }
}