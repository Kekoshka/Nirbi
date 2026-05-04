using MinorTaskService.DataAccess.Postgres.Models;
using MinorTaskService.WebApi.Common.DTO;
using MinorTaskService.WebApi.Mediator;
using Riok.Mapperly.Abstractions;

namespace MinorTaskService.WebApi.Common.Mappers;

[Mapper]
public static partial class MinorTaskMapper
{
    [MapperIgnoreSource(nameof(CreateMinorTaskCommand.Images))]
    public static partial CreateMinorTaskDTO ToCreateMinorTaskDTO(this CreateMinorTaskCommand value);

    [MapProperty(nameof(MinorTask.Status.Name), nameof(GetMinorTaskDTO.Status))]
    public static partial GetMinorTaskDTO ToGetMinorTaskDTO(this MinorTask value);

    [MapProperty(nameof(GetMinorTaskDTO.ConsumerId), nameof(GetMinorTaskResponse.ConsumerId), Use = nameof(FormatConsumerId))]
    public static partial GetMinorTaskResponse ToGetMinorTaskResponse(this GetMinorTaskDTO value);

    private static string FormatConsumerId(Guid consumerId) => consumerId.ToString();

    public static partial UpdateMinorTaskDTO ToUpdateMinorTaskDTO(this UpdateMinorTaskCommand value);

    public static partial List<GetMinorTasksResponse> ToGetMinorTasksResponse(this List<GetMinorTasksDTO> value);

    public static partial IQueryable<GetMinorTasksDTO> ToGetMinorTasksDTO(this IQueryable<MinorTask> value);

    [MapperIgnoreTarget(nameof(MinorTask.Id))]
    public static partial void UpdateMinorTask(UpdateMinorTaskDTO source, MinorTask target);
}
