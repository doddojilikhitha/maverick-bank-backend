namespace MaverickBank.Core.DTOs
{
    public class LoanApplyDTO
    {
        public int UserId { get; set; }
        
        public int AccountId { get; set; }
        
        public int LoanProductId { get; set; }
  
        public decimal AmountApplied { get; set; }

        public string? Purpose { get; set; }
    }

    public class LoanResponseDTO
    {
        public int LoanId { get; set; }
        public int UserId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal AmountApplied { get; set; }
        public decimal InterestRate { get; set; }
        public int TenureMonths { get; set; }
        public string? Purpose { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedOn { get; set; }
        public DateTime? DisbursedOn { get; set; }
    }

    public class LoanProductResponseDTO
    {
        public int LoanProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal LoanAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TenureMonths { get; set; }
    }
}