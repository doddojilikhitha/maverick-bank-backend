namespace MaverickBank.Core.Entities
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public int AccountId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int? ToAccountId { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        public Account Account { get; set; } = null!;
        public Account? ToAccount { get; set; }
    }
}