using MediatR;
using MinorTaskService.WebApi.Interfaces;

namespace MinorTaskService.WebApi.Mediator
{
    public record class UpdateMinorTaskStatusCommand(
        Guid MinorTaskId,
        Guid StatusId) : IRequest;

    public class UpdateMinorTaskStatusCommandHandler : IRequestHandler<UpdateMinorTaskStatusCommand>
    {
        IMinorTaskService _minorTaskService;
        public UpdateMinorTaskStatusCommandHandler(IMinorTaskService minorTaskService)
        {
            _minorTaskService = minorTaskService;
        }

        public async Task Handle(UpdateMinorTaskStatusCommand request, CancellationToken cancellationToken)
        {
            await _minorTaskService.UpdateMinorTaskStatusAsync(request.MinorTaskId, request.StatusId,cancellationToken);
        }
    }
}
