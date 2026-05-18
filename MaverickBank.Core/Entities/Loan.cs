namespace MaverickBank.Core.Entities
{
    public class Loan
    {
        public int LoanId { get; set; }
        public int UserId { get; set; }
        public int LoanProductId { get; set; }
        public int AccountId { get; set; }
        public decimal AmountApplied { get; set; }
        public string? Purpose { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime AppliedOn { get; set; } = DateTime.UtcNow;
        public int? ReviewedBy { get; set; }
        public DateTime? ReviewedOn { get; set; }
        public DateTime? DisbursedOn { get; set; }

        public User User { get; set; } = null!;
        public LoanProduct LoanProduct { get; set; } = null!;
        public Account Account { get; set; } = null!;
    }
}