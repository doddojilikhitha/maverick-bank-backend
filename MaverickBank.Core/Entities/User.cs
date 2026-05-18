namespace MaverickBank.Core.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer";
        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? AadharNo { get; set; }
        public string? PANNo { get; set; }
        public DateTime? DOB { get; set; }
        public int? Age => DOB.HasValue
            ? (int)((DateTime.Today - DOB.Value).TotalDays / 365.25)
            : null;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Account> Accounts { get; set; } = new List<Account>();
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
        public ICollection<Beneficiary> Beneficiaries { get; set; } = new List<Beneficiary>();
    }
}