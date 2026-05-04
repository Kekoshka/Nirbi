using DataService.WebApi.Interfaces;
using MediatR;

namespace DataService.WebApi.Mediator;

public sealed record CreateCollectionCommand : IRequest<Guid>;

public sealed class CreateCollectionCommandHandler : IRequestHandler<CreateCollectionCommand, Guid>
{
    private readonly IDataObjectService _data;

    public CreateCollectionCommandHandler(IDataObjectService data) => _data = data;

    public Task<Guid> Handle(CreateCollectionCommand request, CancellationToken cancellationToken) =>
        _data.CreateCollectionAsync(cancellationToken);
}
