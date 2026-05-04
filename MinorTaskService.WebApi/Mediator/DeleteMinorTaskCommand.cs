using MediatR;
using MinorTaskService.WebApi.Interfaces;

namespace MinorTaskService.WebApi.Mediator
{
    public record class DeleteMinorTaskCommand(
        Guid MinorTaskId) : IRequest;

    public class DeleteMinorTaskCommandHandler : IRequestHandler<DeleteMinorTaskCommand>
    {
        IMinorTaskService _minorTaskService;
        public DeleteMinorTaskCommandHandler(IMinorTaskService minorTaskService)
        {
            _minorTaskService = minorTaskService;
        }
        public async Task Handle(DeleteMinorTaskCommand request, CancellationToken cancellationToken)
        {
            await _minorTaskService.DeleteMinorTaskAsync(request.MinorTaskId, cancellationToken);
        }
    }
}
