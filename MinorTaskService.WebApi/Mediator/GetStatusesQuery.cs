using MediatR;
using MinorTaskService.DataAccess.Postgres.Models;
using MinorTaskService.WebApi.Common.DTO;
using MinorTaskService.WebApi.Common.Mappers;
using MinorTaskService.WebApi.Interfaces;

namespace MinorTaskService.WebApi.Mediator
{
    public record class GetStatusesQuery : IRequest<List<GetStatusesResponse>>;

    public class GetStatusesQueryHandler : IRequestHandler<GetStatusesQuery, List<GetStatusesResponse>>
    {
        IStatusService _statusService;
        public GetStatusesQueryHandler(IStatusService statusService) 
        {
            _statusService = statusService;
        }

        public async Task<List<GetStatusesResponse>> Handle(GetStatusesQuery request, CancellationToken cancellationToken)
        {
            var statuses = await _statusService.GetStatusesAsync(cancellationToken);
            return statuses.ToGetStatusesResponse();
        }
    }
}
