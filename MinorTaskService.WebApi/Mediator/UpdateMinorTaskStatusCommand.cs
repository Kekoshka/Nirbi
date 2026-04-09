using MediatR;

namespace MinorTaskService.WebApi.Mediator
{
    public record class UpdateMinorTaskStatusCommand(
        Guid Id,
        Guid StatusId) : IRequest;

    public class UpdateMinorTaskStatusCommandHandler : IRequestHandler<UpdateMinorTaskStatusCommand>
    {
        public UpdateMinorTaskStatusCommandHandler()
        {

        }

        public Task Handle(UpdateMinorTaskStatusCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
