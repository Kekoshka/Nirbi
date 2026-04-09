using MediatR;

namespace MinorTaskService.WebApi.Mediator
{
    public record class DeleteMinorTaskCommand(
        Guid MinorTaskId) : IRequest;

    public class DeleteMinorTaskCommandHandler : IRequestHandler<DeleteMinorTaskCommand>
    {
        public DeleteMinorTaskCommandHandler()
        {

        }
        public Task Handle(DeleteMinorTaskCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
