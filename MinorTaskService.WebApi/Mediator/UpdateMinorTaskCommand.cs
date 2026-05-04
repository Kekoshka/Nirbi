using MediatR;
using MinorTaskService.WebApi.Common.Mappers;
using MinorTaskService.WebApi.Interfaces;

namespace MinorTaskService.WebApi.Mediator
{
    public record class UpdateMinorTaskCommand(
        Guid Id,
        string Name,
        string Description,
        decimal Latitude,
        decimal Longitude,
        int NumberVolunteers,
        decimal Encouragement) : IRequest;

    public class UpdateMinorTaskCommandHandler : IRequestHandler<UpdateMinorTaskCommand>
    {
        IMinorTaskService _minorTaskService;
        public UpdateMinorTaskCommandHandler(IMinorTaskService minorTaskService)
        {
            _minorTaskService = minorTaskService;
        }
        
        public async Task Handle(UpdateMinorTaskCommand request, CancellationToken cancellationToken)
        {
            await _minorTaskService.UpdateMinorTaskAsync(request.ToUpdateMinorTaskDTO(), cancellationToken);
        }
    }
}
