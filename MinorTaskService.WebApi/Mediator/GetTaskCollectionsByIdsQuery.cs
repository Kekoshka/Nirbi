namespace MinorTaskService.WebApi.Mediator;

using MediatR;
using MinorTaskService.WebApi.Common.DTO;
using MinorTaskService.WebApi.Interfaces;

public record GetTaskCollectionsByIdsQuery(IReadOnlyCollection<Guid> Ids)
    : IRequest<List<TaskCollectionDTO>>;

public class GetTaskCollectionsByIdsQueryHandler
    : IRequestHandler<GetTaskCollectionsByIdsQuery, List<TaskCollectionDTO>>
{
    private readonly IMinorTaskService _service;

    public GetTaskCollectionsByIdsQueryHandler(IMinorTaskService service)
        => _service = service;

    public Task<List<TaskCollectionDTO>> Handle(
        GetTaskCollectionsByIdsQuery request, CancellationToken cancellationToken)
        => _service.GetTaskCollectionsByIdsAsync(request.Ids, cancellationToken);
}