using MediatR;

namespace MinorTaskService.WebApi.Mediator
{
    public record class UpdateMinorTaskCommand(
        Guid Id,
        string Name,
        string Description,
        decimal Latitude,
        decimal Longitude,
        int NimberVolunteers,
        decimal Encouragement,
        Guid StatusId) : IRequest;

    public class UpdateMinorTaskCommandHandler : IRequestHandler<UpdateMinorTaskCommand>
    {

        public UpdateMinorTaskCommandHandler()
        {

        }
        
        public Task Handle(UpdateMinorTaskCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
