namespace MaverickBank.Core.Entities
{
    public class Account
    {
        public int AccountId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string AccountType { get; set; } = "Savings";
        public decimal Balance { get; set; } = 0;
        public string? IFSCCode { get; set; }
        public string? BranchName { get; set; }
        public string? BranchAddress { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}