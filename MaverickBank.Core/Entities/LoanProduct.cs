namespace MaverickBank.Core.Entities
{
    public class LoanProduct
    {
        public int LoanProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal LoanAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TenureMonths { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}