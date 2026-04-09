using MediatR;
using MinorTaskService.WebApi.Common.Mappers;
using MinorTaskService.WebApi.Interfaces;

namespace MinorTaskService.WebApi.Mediator
{
    public record class CreateMinorTaskCommand(
        string Name,
        string Description,
        decimal Latitude,
        decimal Longitude,
        int NimberVolunteers,
        decimal Encouragement,
        List<byte[]> Images) : IRequest<Guid>;

    public class CreateMinorTaskCommandHandler : IRequestHandler<CreateMinorTaskCommand, Guid>
    {
        IMinorTaskService _minorTaskService;
        public CreateMinorTaskCommandHandler(IMinorTaskService minorTaskService)
        {
            _minorTaskService = minorTaskService;
        }

        public async Task<Guid> Handle(CreateMinorTaskCommand request, CancellationToken cancellationToken)
        {
            var dto = request.ToCreateMinorTaskDTO();
            return await _minorTaskService.CreateMinorTaskAsync(dto,cancellationToken);
        }
    }
}
