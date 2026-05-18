using Asp.Versioning;
using MaverickBank.Core.DTOs;
using MaverickBank.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MaverickBank.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IReportService reportService, ILogger<ReportController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        [HttpGet("statement/{accountId}")]
        [Authorize(Roles = "Customer,Employee")]
        public async Task<IActionResult> Statement(int accountId)
        {
            _logger.LogInformation("Account statement requested for AccountId: {AccountId}", accountId);
            var result = await _reportService.GetAccountStatementAsync(accountId);
            return Ok(ApiResponseDTO<List<TransactionResponseDTO>>.Ok(result));
        }

        [HttpGet("performance")]
        [Authorize(Roles = "Employee,Admin")]
        public async Task<IActionResult> Performance()
        {
            _logger.LogInformation("Financial performance report requested.");
            var result = await _reportService.GetFinancialPerformanceAsync();
            return Ok(ApiResponseDTO<TransactionSummaryDTO>.Ok(result));
        }
    }
}