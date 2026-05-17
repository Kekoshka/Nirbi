using DataService.WebApi.Common.DTO;
using DataService.WebApi.Interfaces;
using MediatR;

namespace DataService.WebApi.Mediator;

public sealed record ListCollectionFilesQuery(Guid CollectionId) : IRequest<List<FileMetadataDto>>;

public sealed class ListCollectionFilesQueryHandler : IRequestHandler<ListCollectionFilesQuery, List<FileMetadataDto>>
{
    private readonly IDataObjectService _data;

    public ListCollectionFilesQueryHandler(IDataObjectService data) => _data = data;

    public Task<List<FileMetadataDto>> Handle(ListCollectionFilesQuery request, CancellationToken cancellationToken) =>
        _data.ListByCollectionAsync(request.CollectionId, cancellationToken);
}
