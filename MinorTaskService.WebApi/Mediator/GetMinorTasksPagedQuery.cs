namespace MinorTaskService.WebApi.Mediator;

using MediatR;
using MinorTaskService.WebApi.Common.DTO;
using MinorTaskService.WebApi.Interfaces;

public record GetMinorTasksPagedQuery(
    int Offset, int Limit, string? Search, string? Status, string? Sort)
    : IRequest<PagedMinorTasksDTO>;

public class GetMinorTasksPagedQueryHandler
    : IRequestHandler<GetMinorTasksPagedQuery, PagedMinorTasksDTO>
{
    private readonly IMinorTaskService _service;
    public GetMinorTasksPagedQueryHandler(IMinorTaskService service) => _service = service;

    public Task<PagedMinorTasksDTO> Handle(
        GetMinorTasksPagedQuery request, CancellationToken cancellationToken)
        => _service.GetMinorTasksPagedAsync(
            request.Offset, request.Limit, request.Search, request.Status, request.Sort,
            cancellationToken);
}
