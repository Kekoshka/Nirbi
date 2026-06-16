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
        return Ok(id);
    }

    [HttpGet("tasks/{minorTaskId:guid}")]
    public async Task<IActionResult> GetTask(Guid minorTaskId, CancellationToken cancellationToken)
    {
        var minorTask = await _mediator.Send(new GetMinorTaskByIdQuery(minorTaskId), cancellationToken);
        return Ok(minorTask);
    }

    [HttpPost("tasks/names")]
    public async Task<IActionResult> GetTaskNames(
        GetTaskNamesByIdsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetTasksByIdsQuery(request.Ids ?? []), cancellationToken);
        return Ok(result);
    }

    [HttpGet("tasks")]
    public async Task<IActionResult> GetTasks(
        int offset,
        int limit,
        string? search,
        string? status,
        string? sort,
        CancellationToken cancellationToken)
    {
        if (limit <= 0) limit = 20;
        var result = await _mediator.Send(
            new GetMinorTasksPagedQuery(offset, limit, search, status, sort), cancellationToken);
        return Ok(result);
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

    /// <summary>Батч: по списку ID задач вернуть их FileCollectionId.</summary>
    [HttpPost("tasks/collections")]
    public async Task<IActionResult> GetTaskCollections(
        GetTaskCollectionsByIdsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetTaskCollectionsByIdsQuery(request.Ids ?? []), cancellationToken);
        return Ok(result);
    }

}
