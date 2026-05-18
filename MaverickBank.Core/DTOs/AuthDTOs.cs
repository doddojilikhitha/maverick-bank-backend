namespace MaverickBank.Core.DTOs
{
    public class RegisterDTO
    {
        
        public string FullName { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
    
        public string Password { get; set; } = string.Empty;

        public string? Phone { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? AadharNo { get; set; }
        public string? PANNo { get; set; }
        public DateTime? DOB { get; set; }
        public string Role { get; set; } = "Customer";
    }

    public class LoginDTO
    {
        
        public string Email { get; set; } = string.Empty;

        
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}