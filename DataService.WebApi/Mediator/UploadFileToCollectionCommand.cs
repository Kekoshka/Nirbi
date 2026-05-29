using DataService.WebApi.Interfaces;
using MediatR;

namespace DataService.WebApi.Mediator;

public sealed record UploadFileToCollectionCommand(
    Guid CollectionId,
    Stream Content,
    string ContentType,
    string? OriginalFileName,
    long? KnownSizeBytes,
    bool IsPublic) : IRequest<Guid>;

public sealed class UploadFileToCollectionCommandHandler : IRequestHandler<UploadFileToCollectionCommand, Guid>
{
    private readonly IDataObjectService _data;

    public UploadFileToCollectionCommandHandler(IDataObjectService data) => _data = data;

    public Task<Guid> Handle(UploadFileToCollectionCommand request, CancellationToken cancellationToken) =>
        _data.UploadToCollectionAsync(
            request.CollectionId,
            request.Content,
            request.ContentType,
            request.OriginalFileName,
            request.KnownSizeBytes,
            request.IsPublic,
            cancellationToken);
}
