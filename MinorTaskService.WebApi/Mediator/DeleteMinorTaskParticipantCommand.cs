using MediatR;

namespace MinorTaskService.WebApi.Mediator
{
    public record class DeleteMinorTaskParticipantCommand(
        Guid MinorTaskId,
        Guid ParticipantId) : IRequest;

    public class DeleteMinorTaskParticipantCommandHandler : IRequestHandler<DeleteMinorTaskParticipantCommand>
    {
        public DeleteMinorTaskParticipantCommandHandler()
        {

        }

        public Task Handle(DeleteMinorTaskParticipantCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
