using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinorTaskService.WebApi.Interfaces;

namespace MinorTaskService.WebApi.Controllers;

// Прямой вызов сервиса участников (без MediatR). Добавление участника здесь НЕ
// выставляется наружу — оно происходит только через confirmation-flow.
[Route("api/tasks")]
[ApiController]
[Authorize]
public class TaskParticipantsController : ControllerBase
{
    private readonly ITaskParticipantService _participants;

    public TaskParticipantsController(ITaskParticipantService participants)
    {
        _participants = participants;
    }

    /// <summary>Список участников задачи (только создатель задачи или участник).</summary>
    [HttpGet("{minorTaskId:guid}/participants")]
    [ProducesResponseType(typeof(List<Guid>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetParticipants(Guid minorTaskId)
    {
        var result = await _participants.GetMinorTaskParticipants(minorTaskId);
        return Ok(result);
    }

    /// <summary>Исключить участника (создатель задачи — любого; участник — себя).</summary>
    [HttpDelete("{minorTaskId:guid}/participants/{participantId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveParticipant(
        Guid minorTaskId, Guid participantId, CancellationToken cancellationToken)
    {
        await _participants.RemoveTaskParticipantAsync(minorTaskId, participantId, cancellationToken);
        return NoContent();
    }
}