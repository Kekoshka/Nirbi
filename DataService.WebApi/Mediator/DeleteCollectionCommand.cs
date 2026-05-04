using DataService.WebApi.Interfaces;
using MediatR;

namespace DataService.WebApi.Mediator;

public sealed record DeleteCollectionCommand(Guid CollectionId) : IRequest;

public sealed class DeleteCollectionCommandHandler : IRequestHandler<DeleteCollectionCommand>
{
    private readonly IDataObjectService _data;

    public DeleteCollectionCommandHandler(IDataObjectService data) => _data = data;

    public Task Handle(DeleteCollectionCommand request, CancellationToken cancellationToken) =>
        _data.DeleteCollectionAsync(request.CollectionId, cancellationToken);
}
