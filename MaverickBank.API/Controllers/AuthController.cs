using Asp.Versioning;
using MaverickBank.Core.DTOs;
using MaverickBank.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MaverickBank.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService auth, ILogger<AuthController> logger)
        {
            _auth = auth;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Age check before calling service
            if (dto.DOB.HasValue)
            {
                int age = (int)((DateTime.Today - dto.DOB.Value).TotalDays / 365.25);
                if (age < 18)
                {
                    _logger.LogWarning("Registration rejected - underage: {Email}", dto.Email);
                    return BadRequest(ApiResponseDTO<string>.Fail(
                        "You must be at least 18 years old to register."));
                }
            }

            if (dto.Role != "Customer")
                return BadRequest(ApiResponseDTO<string>.Fail(
                    "Only Customer accounts can self-register."));

            var result = await _auth.RegisterAsync(dto);
            if (!result)
                return BadRequest(ApiResponseDTO<string>.Fail("Email already registered."));

            return Ok(ApiResponseDTO<string>.Ok("Registered successfully. Please login."));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _logger.LogInformation("Login attempt for: {Email}", dto.Email);
            var result = await _auth.LoginAsync(dto);
            if (result == null)
            {
                _logger.LogWarning("Login failed for: {Email}", dto.Email);
                return Unauthorized(ApiResponseDTO<string>.Fail("Invalid email or password.", 401));
            }

            _logger.LogInformation("Login successful for: {Email}", dto.Email);
            return Ok(ApiResponseDTO<AuthResponseDTO>.Ok(result, "Login successful."));
        }
    }
}