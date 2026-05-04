using DataService.WebApi.Common.DTO;
using DataService.WebApi.Interfaces;
using MediatR;

namespace DataService.WebApi.Mediator;

public sealed record GetFileMetadataQuery(Guid FileId) : IRequest<FileMetadataDto?>;

public sealed class GetFileMetadataQueryHandler : IRequestHandler<GetFileMetadataQuery, FileMetadataDto?>
{
    private readonly IDataObjectService _data;

    public GetFileMetadataQueryHandler(IDataObjectService data) => _data = data;

    public Task<FileMetadataDto?> Handle(GetFileMetadataQuery request, CancellationToken cancellationToken) =>
        _data.GetMetadataAsync(request.FileId, cancellationToken);
}
