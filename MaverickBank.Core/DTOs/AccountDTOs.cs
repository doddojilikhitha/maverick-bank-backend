namespace MaverickBank.Core.DTOs
{
    public class OpenAccountDTO
    {
        public int UserId { get; set; }

        public string AccountType { get; set; } = "Savings";

        public string? BranchName { get; set; }
        public string? IFSCCode { get; set; }
        public string? BranchAddress { get; set; }
    }

    public class AccountResponseDTO
    {
        public int AccountId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string? IFSCCode { get; set; }
        public string? BranchName { get; set; }
        public string Status { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class BeneficiaryDTO
    {
        public int UserId { get; set; }

        public string AccountName { get; set; } = string.Empty;

        public string AccountNumber { get; set; } = string.Empty;

        public string? BankName { get; set; }
        public string? BranchName { get; set; }
        public string? IFSCCode { get; set; }
    }
}