namespace MaverickBank.Core.Entities
{
    public class Beneficiary
    {
        public int BeneficiaryId { get; set; }
        public int UserId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string? BankName { get; set; }
        public string? BranchName { get; set; }
        public string? IFSCCode { get; set; }

        public User User { get; set; } = null!;
    }
}