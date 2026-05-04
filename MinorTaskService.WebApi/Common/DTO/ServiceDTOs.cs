namespace MinorTaskService.WebApi.Common.DTO
{
    public record class CreateMinorTaskDTO(
        string Name,
        string Description,
        decimal Latitude,
        decimal Longitude,
        int NumberVolunteers,
        decimal Encouragement);

    public record class UpdateMinorTaskDTO(
        Guid Id,
        string Name,
        string Description,
        decimal Latitude,
        decimal Longitude,
        int NumberVolunteers,
        decimal Encouragement);
    public record class GetMinorTaskDTO(
        string Name,
        string Description,
        decimal Latitude,
        decimal Longitude,
        int NumberVolunteers,
        decimal Encouragement,
        string Status,
        Guid ConsumerId,
        DateTime CreatedAt);
    public record class GetMinorTasksDTO(
        string Name,
        string Description,
        decimal Latitude,
        decimal Longitude,
        decimal Encouragement,
        DateTime CreatedAt);

    public record class GetStatusesDTO(
        Guid Id,
        string Name);

    public record class GetMinorTaskParticipantsDTO(Guid UserId);
}
