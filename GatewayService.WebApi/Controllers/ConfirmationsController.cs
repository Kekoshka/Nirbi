    using GatewayService.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GatewayService.WebApi.Controllers;

/// <summary>
/// Перехватывает GET /api/Confirmations/reviewer и /initiator, обогащает данными
/// из AuthService (username) и MinorTaskService (название задачи).
/// POST/respond/revoke маршруты обрабатываются YARP напрямую.
/// </summary>
[ApiController]
[Route("api/Confirmations")]
[Authorize]
public class ConfirmationsController : ControllerBase
{
    private readonly IConfirmationsAggregator _aggregator;

    public ConfirmationsController(IConfirmationsAggregator aggregator)
    {
        _aggregator = aggregator;
    }

    /// <summary>Входящие подтверждения (я reviewer) — обогащённые.</summary>
    [HttpGet("reviewer/{reviewerId:guid}")]
    public async Task<IActionResult> GetByReviewer(Guid reviewerId)
    {
        var authHeader = Request.Headers.Authorization.ToString();
        var result = await _aggregator.GetByReviewerAsync(reviewerId, authHeader);
        return Ok(result);
    }

    /// <summary>Исходящие подтверждения (я initiator) — обогащённые.</summary>
    [HttpGet("initiator/{initiatorId:guid}")]
    public async Task<IActionResult> GetByInitiator(Guid initiatorId)
    {
        var authHeader = Request.Headers.Authorization.ToString();
        var result = await _aggregator.GetByInitiatorAsync(initiatorId, authHeader);
        return Ok(result);
    }
}
