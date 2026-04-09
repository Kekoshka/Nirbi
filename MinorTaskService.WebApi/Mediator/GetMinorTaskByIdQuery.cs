using MediatR;
using MinorTaskService.WebApi.Common.DTO;

namespace MinorTaskService.WebApi.Mediator
{
    public record class GetMinorTaskByIdQuery(
        Guid MinorTaskId) : IRequest<GetMinorTaskResponse>;

    public class GetMinorTaskByIdQueryHandler : IRequestHandler<GetMinorTaskByIdQuery, GetMinorTaskResponse>
    {
        public GetMinorTaskByIdQueryHandler()
        {

        }

        public Task<GetMinorTaskResponse> Handle(GetMinorTaskByIdQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
