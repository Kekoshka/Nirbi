using GatewayService.WebApi.Common.DTO;
using GatewayService.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GatewayService.WebApi.Controllers;

[ApiController]
[Route("api/tasks")]
public class MinorTasksController : ControllerBase
{
    private readonly IMinorTaskAggregator _aggregator;

    public MinorTasksController(IMinorTaskAggregator aggregator)
    {
        _aggregator = aggregator;
    }

    /// <summary>
    /// Создать задачу: автоматически создаёт коллекцию файлов,
    /// загружает изображения, затем создаёт minor-task.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(MinorTaskDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTask([FromForm] CreateMinorTaskGatewayRequest request)
    {
        var authHeader = Request.Headers.Authorization.ToString();
        var result = await _aggregator.CreateTaskWithFilesAsync(request, authHeader);

        if (result is null)
            return BadRequest("Не удалось создать задачу.");

        return CreatedAtAction(nameof(GetTask), new { minorTaskId = result.Id }, result);
    }

    /// <summary>Получить список задач — с превью первого изображения для каждой.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<MinorTaskListItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks(
        [FromQuery] int? limit,
        [FromQuery] int? from,
        [FromQuery] int? to)
    {
        var authHeader = Request.Headers.Authorization.ToString();
        var result = await _aggregator.GetTasksWithPreviewAsync(limit, from, to, authHeader);
        return Ok(result);
    }

    /// <summary>Получить задачу по ID — с полным списком изображений.</summary>
    [HttpGet("{minorTaskId:guid}")]
    [ProducesResponseType(typeof(MinorTaskDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTask(Guid minorTaskId)
    {
        var authHeader = Request.Headers.Authorization.ToString();
        var result = await _aggregator.GetTaskWithImagesAsync(minorTaskId, authHeader);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>Обновить поля задачи (название, описание, координаты и т.п.)</summary>
    [HttpPatch("{minorTaskId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTask(
        Guid minorTaskId,
        [FromBody] UpdateMinorTaskGatewayRequest request)
    {
        var authHeader = Request.Headers.Authorization.ToString();
        var statusCode = await _aggregator.UpdateTaskAsync(minorTaskId, request, authHeader);
        return StatusCode((int)statusCode);
    }

    /// <summary>Изменить статус задачи.</summary>
    [HttpPut("{minorTaskId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTaskStatus(
        Guid minorTaskId,
        [FromBody] UpdateMinorTaskStatusGatewayRequest request)
    {
        var authHeader = Request.Headers.Authorization.ToString();
        var statusCode = await _aggregator.UpdateTaskStatusAsync(minorTaskId, request.StatusId, authHeader);
        return StatusCode((int)statusCode);
    }

    /// <summary>Удалить задачу.</summary>
    [HttpDelete("{minorTaskId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(Guid minorTaskId)
    {
        var authHeader = Request.Headers.Authorization.ToString();
        var statusCode = await _aggregator.DeleteTaskAsync(minorTaskId, authHeader);
        return StatusCode((int)statusCode);
    }

    /// <summary>Удалить участника из задачи.</summary>
    [HttpDelete("{minorTaskId:guid}/participants/{participantId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTaskParticipant(Guid minorTaskId, Guid participantId)
    {
        var authHeader = Request.Headers.Authorization.ToString();
        var statusCode = await _aggregator.DeleteTaskParticipantAsync(minorTaskId, participantId, authHeader);
        return StatusCode((int)statusCode);
    }
}