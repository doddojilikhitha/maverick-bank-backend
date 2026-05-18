namespace MaverickBank.Core.DTOs
{
    public class DepositWithdrawDTO
    {
        
        public int AccountId { get; set; }

        public decimal Amount { get; set; }

        public string? Description { get; set; }
    }

    public class TransferDTO
    {
       
        public int FromAccountId { get; set; }
      
        public int ToAccountId { get; set; }

        public int UserId { get; set; }
    
        public decimal Amount { get; set; }

        public string? Description { get; set; }
    }

    public class TransactionResponseDTO
    {
        public int TransactionId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public int AccountId { get; set; }
        public int? ToAccountId { get; set; }
    }

    public class TransactionSummaryDTO
    {
        public decimal TotalInbound { get; set; }
        public decimal TotalOutbound { get; set; }
        public List<TransactionResponseDTO> Transactions { get; set; } = new();
    }
}