using Asp.Versioning;
using MaverickBank.Core.DTOs;
using MaverickBank.Core.Interfaces;
using MaverickBank.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaverickBank.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _db;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAuthService authService, AppDbContext db, ILogger<AdminController> logger)
        {
            _authService = authService;
            _db = db;
            _logger = logger;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _db.Users
                .Select(u => new { u.UserId, u.FullName, u.Email, u.Role, u.IsActive, u.CreatedAt })
                .ToListAsync();
            return Ok(ApiResponseDTO<object>.Ok(users));
        }

        [HttpPost("employee")]
        public async Task<IActionResult> AddEmployee([FromBody] RegisterDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            dto.Role = "Employee";
            _logger.LogInformation("Admin creating employee: {Email}", dto.Email);
            var result = await _authService.RegisterAsync(dto);
            if (!result) return BadRequest(ApiResponseDTO<string>.Fail("Employee with this email already exists."));
            return Ok(ApiResponseDTO<string>.Ok("Employee created successfully."));
        }

        [HttpPut("user/{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound(ApiResponseDTO<string>.Fail("User not found.", 404));
            user.IsActive = false;
            await _db.SaveChangesAsync();
            _logger.LogInformation("User deactivated: {UserId}", id);
            return Ok(ApiResponseDTO<string>.Ok("User deactivated."));
        }

        [HttpPut("user/{id}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound(ApiResponseDTO<string>.Fail("User not found.", 404));
            user.IsActive = true;
            await _db.SaveChangesAsync();
            _logger.LogInformation("User activated: {UserId}", id);
            return Ok(ApiResponseDTO<string>.Ok("User activated."));
        }
    }
}
