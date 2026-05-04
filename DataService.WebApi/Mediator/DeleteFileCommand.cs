using DataService.WebApi.Interfaces;
using MediatR;

namespace DataService.WebApi.Mediator;

public sealed record DeleteFileCommand(Guid FileId) : IRequest;

public sealed class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand>
{
    private readonly IDataObjectService _data;

    public DeleteFileCommandHandler(IDataObjectService data) => _data = data;

    public Task Handle(DeleteFileCommand request, CancellationToken cancellationToken) =>
        _data.DeleteFileAsync(request.FileId, cancellationToken);
}
