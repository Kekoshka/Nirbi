using GatewayService.WebApi.Common.DTO;
using GatewayService.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GatewayService.WebApi.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
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

    /// <summary>
    /// Получить список задач. Вместо FileCollectionId подставляется
    /// URL первого изображения (PreviewImageUrl).
    /// </summary>
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

    /// <summary>
    /// Получить задачу по ID. Вместо FileCollectionId подставляется
    /// полный список изображений с метаданными и ссылками на скачивание.
    /// </summary>
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
}
