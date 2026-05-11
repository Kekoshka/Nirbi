using MediatR;
using MinorTaskService.WebApi.Common.Mappers;
using MinorTaskService.WebApi.Interfaces;

namespace MinorTaskService.WebApi.Mediator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Name">Название</param>
    /// <param name="Description">Описание</param>
    /// <param name="Latitude">Координаты по широте</param>
    /// <param name="Longitude">Координаты по долготе</param>
    /// <param name="NumberVolunteers">Количество волонтеров</param>
    /// <param name="Encouragement">Поощрение</param>
    /// <param name="Images">Guid ListData с изображениями из Data сервиса</param>
    public record class CreateMinorTaskCommand(
        string Name,
        string Description,
        decimal Latitude,
        decimal Longitude,
        int NumberVolunteers,
        decimal Encouragement,
        Guid FileCollectionId) : IRequest<Guid>;

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
