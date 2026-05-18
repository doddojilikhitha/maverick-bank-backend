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
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _loanService;
        private readonly ILogger<LoanController> _logger;

        public LoanController(ILoanService loanService, ILogger<LoanController> logger)
        {
            _loanService = loanService;
            _logger = logger;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("products")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetProducts()
        {
            var result = await _loanService.GetLoanProductsAsync();
            return Ok(ApiResponseDTO<List<LoanProductResponseDTO>>.Ok(result));
        }

        [HttpPost("apply")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Apply([FromBody] LoanApplyDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            dto.UserId = GetUserId();
            _logger.LogInformation("Loan application by UserId: {UserId}", dto.UserId);
            var result = await _loanService.ApplyLoanAsync(dto);
            if (!result) return BadRequest(ApiResponseDTO<string>.Fail("Loan application failed. Check account or product details."));
            return Ok(ApiResponseDTO<string>.Ok("Loan application submitted successfully."));
        }

        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MyLoans()
        {
            var result = await _loanService.GetMyLoansAsync(GetUserId());
            return Ok(ApiResponseDTO<List<LoanResponseDTO>>.Ok(result));
        }

        [HttpGet("all")]
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> All(
            int pageNumber = 1,
            int pageSize = 10,
            string? status = null,
            string? sortBy = null)
        {
            var result = await _loanService.GetAllLoansAsync(
                pageNumber,
                pageSize,
                status,
                sortBy);

            return Ok(ApiResponseDTO<List<LoanResponseDTO>>.Ok(result));
        }

        [HttpPut("approve/{id}")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Approve(int id)
        {
            _logger.LogInformation("Loan approved by EmployeeId: {EmpId}, LoanId: {LoanId}", GetUserId(), id);
            var result = await _loanService.ApproveLoanAsync(id, GetUserId());
            if (!result) return BadRequest(ApiResponseDTO<string>.Fail("Loan approval failed. Must be in Pending status."));
            return Ok(ApiResponseDTO<string>.Ok("Loan approved."));
        }

        [HttpPut("reject/{id}")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Reject(int id)
        {
            _logger.LogInformation("Loan rejected by EmployeeId: {EmpId}, LoanId: {LoanId}", GetUserId(), id);
            var result = await _loanService.RejectLoanAsync(id, GetUserId());
            if (!result) return BadRequest(ApiResponseDTO<string>.Fail("Loan rejection failed."));
            return Ok(ApiResponseDTO<string>.Ok("Loan rejected."));
        }

        [HttpPut("disburse/{id}")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Disburse(int id)
        {
            _logger.LogInformation("Loan disbursement for LoanId: {LoanId}", id);
            var result = await _loanService.DisburseLoanAsync(id);
            if (!result) return BadRequest(ApiResponseDTO<string>.Fail("Disbursement failed. Loan must be approved first."));
            return Ok(ApiResponseDTO<string>.Ok("Loan disbursed successfully."));
        }
    }
}