using Asp.Versioning;
using MaverickBank.Core.DTOs;
using MaverickBank.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MaverickBank.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyAccounts()
        {
            _logger.LogInformation("Fetching accounts for UserId: {UserId}", GetUserId());
            var result = await _accountService.GetMyAccountsAsync(GetUserId());
            return Ok(ApiResponseDTO<List<AccountResponseDTO>>.Ok(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _accountService.GetAccountByIdAsync(id);
            if (result == null) return NotFound(ApiResponseDTO<string>.Fail("Account not found.", 404));
            return Ok(ApiResponseDTO<AccountResponseDTO>.Ok(result));
        }

        [HttpGet("all")]
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> GetAll(
             int pageNumber = 1,
             int pageSize = 10,
             string? status = null,
             string? sortBy = null)
        {
            var result = await _accountService.GetAllAccountsAsync(
                pageNumber,
                pageSize,
                status,
                sortBy);

            return Ok(ApiResponseDTO<List<AccountResponseDTO>>.Ok(result));
        }

        [HttpPost("open")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Open([FromBody] OpenAccountDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            dto.UserId = GetUserId();
            _logger.LogInformation("Account open request by UserId: {UserId}", dto.UserId);
            var result = await _accountService.OpenAccountAsync(dto);
            if (!result) return BadRequest(ApiResponseDTO<string>.Fail("Unable to open account."));
            return Ok(ApiResponseDTO<string>.Ok("Account opening request submitted for approval."));
        }

        [HttpPost("close-request/{id}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> RequestClose(int id)
        {
            var result = await _accountService.RequestCloseAccountAsync(id);
            if (!result) return BadRequest(ApiResponseDTO<string>.Fail("Unable to request closure. Account must be active."));
            return Ok(ApiResponseDTO<string>.Ok("Account closure request submitted."));
        }

        [HttpPut("approve/{id}")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Approve(int id)
        {
            _logger.LogInformation("Employee approving account: {AccountId}", id);
            var result = await _accountService.ApproveAccountAsync(id);
            if (!result) return BadRequest(ApiResponseDTO<string>.Fail("Unable to approve. Account must be in Pending status."));
            return Ok(ApiResponseDTO<string>.Ok("Account approved successfully."));
        }

        [HttpPut("close/{id}")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Close(int id)
        {
            _logger.LogInformation("Employee closing account: {AccountId}", id);
            var result = await _accountService.CloseAccountAsync(id);
            if (!result) return BadRequest(ApiResponseDTO<string>.Fail("Unable to close account."));
            return Ok(ApiResponseDTO<string>.Ok("Account closed successfully."));
        }

        [HttpPost("beneficiary")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> AddBeneficiary([FromBody] BeneficiaryDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            dto.UserId = GetUserId();
            var result = await _accountService.AddBeneficiaryAsync(dto);
            if (!result) return BadRequest(ApiResponseDTO<string>.Fail("Beneficiary already exists."));
            return Ok(ApiResponseDTO<string>.Ok("Beneficiary added successfully."));
        }

        [HttpGet("beneficiaries")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetBeneficiaries()
        {
            var result = await _accountService.GetBeneficiariesAsync(GetUserId());
            return Ok(ApiResponseDTO<List<BeneficiaryDTO>>.Ok(result));
        }
    }
}