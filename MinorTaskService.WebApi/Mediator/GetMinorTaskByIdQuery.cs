using MediatR;
using MinorTaskService.WebApi.Common.DTO;
using MinorTaskService.WebApi.Common.Mappers;
using MinorTaskService.WebApi.Interfaces;

namespace MinorTaskService.WebApi.Mediator
{
    public record class GetMinorTaskByIdQuery(
        Guid MinorTaskId) : IRequest<GetMinorTaskResponse>;

    public class GetMinorTaskByIdQueryHandler : IRequestHandler<GetMinorTaskByIdQuery, GetMinorTaskResponse>
    {
        IMinorTaskService _minorTaskService;
        public GetMinorTaskByIdQueryHandler(IMinorTaskService minorTaskService)
        {
            _minorTaskService = minorTaskService;
        }

        public async Task<GetMinorTaskResponse> Handle(GetMinorTaskByIdQuery request, CancellationToken cancellationToken)
        {
            var task = await _minorTaskService.GetMinorTaskByIdAsync(request.MinorTaskId);
            return task.ToGetMinorTaskResponse();
        }
    }
}
