using ConfirmationService.WebApi.Common.DTO;
using ConfirmationService.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConfirmationService.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfirmationsController : ControllerBase
{
    private readonly IConfirmationService _confirmationService;
    private readonly ILogger<ConfirmationsController> _logger;

    public ConfirmationsController(
        IConfirmationService confirmationService,
        ILogger<ConfirmationsController> logger)
    {
        _confirmationService = confirmationService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new confirmation request
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ConfirmationResponse>> CreateConfirmation(
        CreateConfirmationRequest request)
    {
        var confirmation = await _confirmationService.CreateConfirmationAsync(request);
        return Ok(confirmation.);
    }

    /// <summary>
    /// Получить подтверждение по ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetConfirmation(Guid id)
    {
        var confirmation = await _confirmationService.GetConfirmationAsync(id);
        return Ok(confirmation);
    }

    /// <summary>
    /// Получить свои подтверждения по id рецензента
    /// </summary>
    [HttpGet("reviewer/{reviewerId}")]
    public async Task<IActionResult> GetByReviewer(Guid reviewerId)
    {
        var confirmations = await _confirmationService.GetConfirmationsByReviewerAsync(reviewerId);
        return Ok(confirmations);
    }


    /// <summary>
    /// Получить свои подтверждения по id инициатора
    /// </summary>
    [HttpGet("initiator/{initiatorId}")]
    public async Task<ActionResult<IEnumerable<ConfirmationResponse>>> GetByInitiator(Guid initiatorId)
    {
        var confirmations = await _confirmationService.GetConfirmationsByInitiatorAsync(initiatorId);
        return Ok(confirmations);
    }

    /// <summary>
    /// Accept or reject confirmation
    /// </summary>
    [HttpPost("{confirmationId}/respond")]
    public async Task<ActionResult<ConfirmationResponse>> RespondToConfirmation(
        Guid confirmationId,
        RespondToConfirmationRequest request)
    {
        var confirmation = await _confirmationService.RespondToConfirmationAsync(confirmationId, request);
        return Ok(confirmation);
    }

    /// <summary>
    /// Revoke confirmation (by initiator)
    /// </summary>
    [HttpPost("{id}/revoke")]
    public async Task<ActionResult<ConfirmationResponse>> RevokeConfirmation(
        Guid id,
        [FromQuery] Guid initiatorId)
    {
        try
        {
            var confirmation = await _confirmationService.RevokeConfirmationAsync(id, initiatorId);
            return Ok(confirmation);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}