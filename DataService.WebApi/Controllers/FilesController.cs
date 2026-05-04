using DataService.WebApi.Common.DTO;
using DataService.WebApi.Mediator;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataService.WebApi.Controllers;

[ApiController]
[Route("api/files")]
//[Authorize]
public class FilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public FilesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [RequestSizeLimit(524_288_000)]
    public async Task<ActionResult<Guid>> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
            return BadRequest("File is empty.");

        await using var stream = file.OpenReadStream();
        var id = await _mediator.Send(
            new UploadStandaloneFileCommand(stream, file.ContentType, file.FileName, file.Length),
            cancellationToken);
        return CreatedAtAction(nameof(GetMetadata), new { id }, id);
    }

    [HttpGet("{id:guid}/metadata")]
    public async Task<ActionResult<FileMetadataDto>> GetMetadata(Guid id, CancellationToken cancellationToken)
    {
        var dto = await _mediator.Send(new GetFileMetadataQuery(id), cancellationToken);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DownloadFileQuery(id), cancellationToken);
        if (result is null)
            return NotFound();

        return File(result.Stream, result.ContentType, result.FileDownloadName);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteFileCommand(id), cancellationToken);
        return NoContent();
    }
}
