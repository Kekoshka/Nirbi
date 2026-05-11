using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinorTaskService.WebApi.Common.DTO;
using MinorTaskService.WebApi.Mediator;

namespace MinorTaskService.WebApi.Controllers;

[Route("api/")]
[ApiController]
[Authorize]
public class MinorTasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public MinorTasksController(IMediator mediator) => _mediator = mediator;

    [HttpPost("tasks")]
    public async Task<IActionResult> CreateTask(CreateMinorTaskRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateMinorTaskCommand(
            request.Name,
            request.Description,
            request.Latitude,
            request.Longitude,
            request.NumberVolunteers,
            request.Encouragement,
            request.Images);
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetTask), new { minorTaskId = id }, id);
    }

    [HttpGet("tasks/{minorTaskId:guid}")]
    public async Task<IActionResult> GetTask(Guid minorTaskId, CancellationToken cancellationToken)
    {
        var minorTask = await _mediator.Send(new GetMinorTaskByIdQuery(minorTaskId), cancellationToken);
        return Ok(minorTask);
    }

    [HttpGet("tasks")]
    public async Task<IActionResult> GetTasks(
        [FromQuery] int? limit,
        [FromQuery] int? from,
        [FromQuery] int? to,
        CancellationToken cancellationToken)
    {
        var minorTasks = await _mediator.Send(new GetMinorTasksQuery(limit, from, to), cancellationToken);
        return Ok(minorTasks);
    }

    [HttpGet("statuses")]
    public async Task<IActionResult> GetStatuses(CancellationToken cancellationToken)
    {
        var statuses = await _mediator.Send(new GetStatusesQuery(), cancellationToken);
        return Ok(statuses);
    }

    [HttpPatch("tasks/{minorTaskId:guid}")]
    public async Task<IActionResult> UpdateTask(
        Guid minorTaskId,
        UpdateMinorTaskRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateMinorTaskCommand(
            minorTaskId,
            request.Name,
            request.Description,
            request.Latitude,
            request.Longitude,
            request.NumberVolunteers,
            request.Encouragement);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPut("tasks/{minorTaskId:guid}")]
    public async Task<IActionResult> UpdateTaskStatus(
        Guid minorTaskId,
        UpdateMinorTaskStatusRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateMinorTaskStatusCommand(minorTaskId, request.StatusId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("tasks/{minorTaskId:guid}")]
    public async Task<IActionResult> DeleteTask(Guid minorTaskId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteMinorTaskCommand(minorTaskId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("tasks/{minorTaskId:guid}/participants/{participantId:guid}")]
    public async Task<IActionResult> DeleteTaskParticipant(
        Guid minorTaskId,
        Guid participantId,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteMinorTaskParticipantCommand(minorTaskId, participantId), cancellationToken);
        return NoContent();
    }
}
