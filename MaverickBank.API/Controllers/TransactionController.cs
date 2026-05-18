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
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _txnService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ITransactionService txnService, ILogger<TransactionController> logger)
        {
            _txnService = txnService;
            _logger = logger;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("deposit")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Deposit([FromBody] DepositWithdrawDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _logger.LogInformation("Deposit AccountId: {Id}, Amount: {Amt}", dto.AccountId, dto.Amount);

            var (success, message) = await _txnService.DepositAsync(dto);
            if (!success) return BadRequest(ApiResponseDTO<string>.Fail(message));
            return Ok(ApiResponseDTO<string>.Ok(message));
        }

        [HttpPost("withdraw")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Withdraw([FromBody] DepositWithdrawDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _logger.LogInformation("Withdrawal AccountId: {Id}, Amount: {Amt}", dto.AccountId, dto.Amount);

            var (success, message) = await _txnService.WithdrawAsync(dto);
            if (!success) return BadRequest(ApiResponseDTO<string>.Fail(message));
            return Ok(ApiResponseDTO<string>.Ok(message));
        }

        [HttpPost("transfer")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Transfer([FromBody] TransferDTO dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            dto.UserId = GetUserId();
            _logger.LogInformation("Transfer from: {From} to: {To}, Amount: {Amt}",
                dto.FromAccountId, dto.ToAccountId, dto.Amount);

            var (success, message) = await _txnService.TransferAsync(dto);
            if (!success) return BadRequest(ApiResponseDTO<string>.Fail(message));
            return Ok(ApiResponseDTO<string>.Ok(message));
        }

        [HttpGet("{accountId}/last10")]
        public async Task<IActionResult> Last10(int accountId)
        {
            var result = await _txnService.GetLast10Async(accountId);
            return Ok(ApiResponseDTO<List<TransactionResponseDTO>>.Ok(result));
        }

        [HttpGet("{accountId}/lastmonth")]
        public async Task<IActionResult> LastMonth(int accountId)
        {
            var result = await _txnService.GetLastMonthAsync(accountId);
            return Ok(ApiResponseDTO<List<TransactionResponseDTO>>.Ok(result));
        }

        [HttpGet("{accountId}/between")]
        public async Task<IActionResult> Between(int accountId, [FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var result = await _txnService.GetBetweenDatesAsync(accountId, from, to);
            return Ok(ApiResponseDTO<List<TransactionResponseDTO>>.Ok(result));
        }
        [HttpGet("all")]
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> GetAll(
            int pageNumber = 1,
            int pageSize = 10,
            string? sortBy = null,
            string? type = null)
        {
            var result = await _txnService.GetAllTransactionsAsync(
                pageNumber, pageSize, sortBy, type);

            return Ok(ApiResponseDTO<List<TransactionResponseDTO>>.Ok(result));
        }

        [HttpGet("{accountId}/summary")]
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> Summary(int accountId)
        {
            var result = await _txnService.GetAccountSummaryAsync(accountId);
            return Ok(ApiResponseDTO<TransactionSummaryDTO>.Ok(result));
        }
    }
}
