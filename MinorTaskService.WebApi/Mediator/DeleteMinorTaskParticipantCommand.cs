using MediatR;
using MinorTaskService.WebApi.Interfaces;

namespace MinorTaskService.WebApi.Mediator
{
    public record class DeleteMinorTaskParticipantCommand(
        Guid MinorTaskId,
        Guid ParticipantId) : IRequest;

    public class DeleteMinorTaskParticipantCommandHandler : IRequestHandler<DeleteMinorTaskParticipantCommand>
    {
        ITaskParticipantService _taskParticipantService;
        public DeleteMinorTaskParticipantCommandHandler(ITaskParticipantService taskParticipantService)
        {
            _taskParticipantService = taskParticipantService;
        }

        public async Task Handle(DeleteMinorTaskParticipantCommand request, CancellationToken cancellationToken)
        {
            await _taskParticipantService.RemoveTaskParticipantAsync(request.MinorTaskId, request.ParticipantId, cancellationToken);
        }
    }
}
