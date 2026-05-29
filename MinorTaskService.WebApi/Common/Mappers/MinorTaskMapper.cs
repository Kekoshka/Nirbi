using MinorTaskService.DataAccess.Postgres.Models;
using MinorTaskService.WebApi.Common.DTO;
using MinorTaskService.WebApi.Mediator;
using Riok.Mapperly.Abstractions;

namespace MinorTaskService.WebApi.Common.Mappers;

[Mapper]
public static partial class MinorTaskMapper
{
    public static partial CreateMinorTaskDTO ToCreateMinorTaskDTO(this CreateMinorTaskCommand value);

    // ── Entity → DTO (single task) ────────────────────────────────────────
    [MapProperty(nameof(MinorTask.Status.Name), nameof(GetMinorTaskDTO.Status))]
    public static partial GetMinorTaskDTO ToGetMinorTaskDTO(this MinorTask value);

    // ── DTO → Response (single task) ──────────────────────────────────────
    [MapProperty(nameof(GetMinorTaskDTO.ConsumerId), nameof(GetMinorTaskResponse.ConsumerId), Use = nameof(FormatConsumerId))]
    public static partial GetMinorTaskResponse ToGetMinorTaskResponse(this GetMinorTaskDTO value);

    public static partial UpdateMinorTaskDTO ToUpdateMinorTaskDTO(this UpdateMinorTaskCommand value);

    // ── DTO → Response (list element) ─────────────────────────────────────
    [MapProperty(nameof(GetMinorTasksDTO.ConsumerId), nameof(GetMinorTasksResponse.ConsumerId), Use = nameof(FormatConsumerId))]
    public static partial GetMinorTasksResponse ToGetMinorTasksResponse(this GetMinorTasksDTO value);

    public static partial List<GetMinorTasksResponse> ToGetMinorTasksResponse(this List<GetMinorTasksDTO> value);

    public static List<GetMinorTasksDTO> ToGetMinorTasksDTO(this List<MinorTask> value) =>
        value.Select(mt => new GetMinorTasksDTO(mt.Id,
            mt.Name,
            mt.Description, 
            mt.Latitude, 
            mt.Longitude, 
            mt.NumberVolunteers, 
            mt.Encouragement, 
            mt.Status.Name, 
            mt.ConsumerId, 
            mt.CreatedAt, 
            mt.FileCollectionId)).ToList();

    private static string FormatConsumerId(Guid consumerId) => consumerId.ToString();

    [MapperIgnoreTarget(nameof(MinorTask.Id))]
    public static partial void UpdateMinorTask(UpdateMinorTaskDTO source, MinorTask target);
}
