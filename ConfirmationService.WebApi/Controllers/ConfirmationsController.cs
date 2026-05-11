using ConfirmationService.WebApi.Common.DTO;
using ConfirmationService.WebApi.Common.DTO.ServiceDTO;
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
        var confirmationId = await _confirmationService.CreateConfirmationAsync(request);
        return Ok(confirmationId);
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
        var confirmations = await _confirmationService.GetConfirmationsByReviewerAsync();
        return Ok(confirmations);
    }


    /// <summary>
    /// Получить свои подтверждения по id инициатора
    /// </summary>
    [HttpGet("initiator/{initiatorId}")]
    public async Task<ActionResult<IEnumerable<ConfirmationResponse>>> GetByInitiator(Guid initiatorId)
    {
        var confirmations = await _confirmationService.GetConfirmationsByInitiatorAsync();
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
        RespondToConfirmationDTO respondToConfirmationDTO = new(
            confirmationId, 
            request.IsAccepted, 
            request.RejectionReason);
        await _confirmationService.RespondToConfirmationAsync(respondToConfirmationDTO);
        return NoContent();
    }

    /// <summary>
    /// Revoke confirmation (by initiator)
    /// </summary>
    [HttpPost("{id}/revoke")]
    public async Task<ActionResult<ConfirmationResponse>> RevokeConfirmation(
        Guid id,
        [FromQuery] Guid initiatorId)
    {
        await _confirmationService.RevokeConfirmationAsync(id);
        return NoContent();
    }
}