using MediatR;
using MinorTaskService.WebApi.Common.DTO;
using MinorTaskService.WebApi.Common.Mappers;
using MinorTaskService.WebApi.Interfaces;

namespace MinorTaskService.WebApi.Mediator
{
    public record class GetMinorTasksQuery(
        int? Limit,
        int? From,
        int? To) :IRequest<List<GetMinorTasksResponse>>;

    public class GetMinorTasksQueryHandler : IRequestHandler<GetMinorTasksQuery, List<GetMinorTasksResponse>>
    {
        IMinorTaskService _minorTaskService;
        public GetMinorTasksQueryHandler(IMinorTaskService minorTaskService)
        {
            _minorTaskService = minorTaskService;
        }

        public async Task<List<GetMinorTasksResponse>> Handle(GetMinorTasksQuery request, CancellationToken cancellationToken)
        {
            List<GetMinorTasksDTO> response;
            if (request.Limit is not null)
                response = await _minorTaskService.GetMinorTasksFirstAsync(request.Limit.Value, cancellationToken);
            else if (request.From is not null && request.To is not null)
                response = await _minorTaskService.GetMinorTasksBetweenAsync(request.From.Value, request.To.Value, cancellationToken);
            else
                throw new ArgumentException("Specify either limit or both from and to query parameters.");

            return response.ToGetMinorTasksResponse();
        }
    }
}
