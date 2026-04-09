namespace MinorTaskService.WebApi.Common.DTO
{
    public record class GetMinorTaskResponse(
        string Name,
        string Description,
        decimal Latitude,
        decimal Longitude,
        int NumberVolunteers,
        decimal Encouragement,
        string Status,
        string ConsumerId,
        DateTime CreatedAt);
    public record class GetMinorTasksResponse(
        string Name,
        string Description,
        decimal Latitude,
        decimal Longitude,
        decimal Encouragement);
}
