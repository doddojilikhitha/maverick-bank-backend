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

            _logger.LogInformation("Register attempt for email: {Email}", dto.Email);
            var result = await _auth.RegisterAsync(dto);
            if (!result)
            {
                _logger.LogWarning("Registration failed - email already exists: {Email}", dto.Email);
                return BadRequest(ApiResponseDTO<string>.Fail("Email already registered."));
            }

            _logger.LogInformation("Registration successful for: {Email}", dto.Email);
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