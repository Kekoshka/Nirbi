using DataService.WebApi.Interfaces;
using MediatR;

namespace DataService.WebApi.Mediator;

public sealed record UploadStandaloneFileCommand(
    Stream Content,
    string ContentType,
    string? OriginalFileName,
    long? KnownSizeBytes) : IRequest<Guid>;

public sealed class UploadStandaloneFileCommandHandler : IRequestHandler<UploadStandaloneFileCommand, Guid>
{
    private readonly IDataObjectService _data;

    public UploadStandaloneFileCommandHandler(IDataObjectService data) => _data = data;

    public Task<Guid> Handle(UploadStandaloneFileCommand request, CancellationToken cancellationToken) =>
        _data.UploadStandaloneAsync(
            request.Content,
            request.ContentType,
            request.OriginalFileName,
            request.KnownSizeBytes,
            cancellationToken);
}
