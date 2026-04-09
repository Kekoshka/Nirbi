using MediatR;
using MinorTaskService.WebApi.Common.DTO;

namespace MinorTaskService.WebApi.Mediator
{
    public class GetMinorTasksQuery(
        int? Limit,
        int? From,
        int? To) :IRequest<GetMinorTasksResponse>;

    public class GetMinorTasksQueryHandler : IRequestHandler<GetMinorTasksQuery, GetMinorTasksResponse>
    {
        public GetMinorTasksQueryHandler()
        {

        }

        public Task<GetMinorTasksResponse> Handle(GetMinorTasksQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
