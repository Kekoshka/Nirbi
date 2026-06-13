using DataService.WebApi.Common.DTO;
using DataService.WebApi.Interfaces;
using DataService.WebApi.Mediator;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataService.WebApi.Controllers;

[ApiController]
[Route("api/collections")]
[Authorize]
public class CollectionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CollectionsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(new CreateCollectionCommand(), cancellationToken);
        return CreatedAtAction(nameof(ListFiles), new { id }, id);
    }

    [HttpGet("{id:guid}/files")]
    public async Task<ActionResult<IReadOnlyList<FileMetadataDto>>> ListFiles(Guid id, CancellationToken cancellationToken)
    {
        var list = await _mediator.Send(new ListCollectionFilesQuery(id), cancellationToken);
        return Ok(list);
    }

    [HttpPost("{id:guid}/files")]
    [RequestSizeLimit(524_288_000)]
    public async Task<ActionResult<Guid>> UploadToCollection(
        Guid id,
        bool isPublic,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
            return BadRequest("File is empty.");

        await using var stream = file.OpenReadStream();
        try
        {
            var fileId = await _mediator.Send(
                new UploadFileToCollectionCommand(id, stream, file.ContentType, file.FileName, file.Length, isPublic),
                cancellationToken);
            return CreatedAtAction(
                nameof(FilesController.GetMetadata),
                "Files",
                new { id = fileId },
                fileId);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCollectionCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("previews")]
    public async Task<ActionResult<IReadOnlyList<CollectionPreviewDto>>> GetPreviews(
        [FromBody] CollectionPreviewRequest request,
        [FromServices] IDataObjectService dataObjectService,
        CancellationToken cancellationToken)
    {
        if (request?.CollectionIds is null || request.CollectionIds.Count == 0)
            return Ok(Array.Empty<CollectionPreviewDto>());

        var previews = await dataObjectService.GetCollectionPreviewsAsync(
            request.CollectionIds, cancellationToken);

        return Ok(previews);
    }
}
