using MediatR;
using MinorTaskService.DataAccess.Postgres.Models;

namespace MinorTaskService.WebApi.Mediator
{
    public record class GetStatusesQuery : IRequest<List<Status>>;

    public class GetStatusesQueryHandler : IRequestHandler<GetStatusesQuery, List<Status>>
    {
        public GetStatusesQueryHandler() 
        {
        
        }

        public Task<List<Status>> Handle(GetStatusesQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
