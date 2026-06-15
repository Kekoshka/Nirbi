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

    /// <summary>Список задач: серверная пагинация, поиск, фильтр по статусу, сортировка.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedTasksResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks(
        int offset,
        int limit,
        string? search,
        string? status,
        string? sort)
    {
        if (limit <= 0) limit = 20;
        var authHeader = Request.Headers.Authorization.ToString();
        var result = await _aggregator.GetTasksPagedAsync(offset, limit, search, status, sort, authHeader);
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

    /// <summary>Список участников задачи с именами (создатель/участник — проверка в MinorTaskService).</summary>
    [HttpGet("{minorTaskId:guid}/participants")]
    [ProducesResponseType(typeof(List<TaskParticipantResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaskParticipants(Guid minorTaskId)
    {
        var authHeader = Request.Headers.Authorization.ToString();
        var result = await _aggregator.GetTaskParticipantsEnrichedAsync(minorTaskId, authHeader);
        return Ok(result);
    }

    /// <summary>
    /// Батч-превью задач для ленивой загрузки. Фронт присылает пачку task ID
    /// (по 5–10), получает для каждой первое изображение в base64.
    /// Задачи без картинок в ответе отсутствуют.
    /// </summary>
    [HttpPost("previews")]
    [ProducesResponseType(typeof(List<TaskPreviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaskPreviews([FromBody] TaskPreviewsRequest request)
    {
        if (request?.TaskIds is null || request.TaskIds.Count == 0)
            return Ok(Array.Empty<TaskPreviewResponse>());

        var authHeader = Request.Headers.Authorization.ToString();
        var result = await _aggregator.GetTaskPreviewsAsync(request.TaskIds, authHeader);
        return Ok(result);
    }
}