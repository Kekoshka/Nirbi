namespace MinorTaskService.WebApi.Common.DTO
{
    public record class UpdateMinorTaskRequest(
        string Name,
        string Description,
        decimal Latitude,
        decimal Longitude,
        int NumberVolunteers,
        decimal Encouragement);
    public record class UpdateMinorTaskStatusRequest(
        Guid StatusId);

    public record class CreateMinorTaskRequest(
        string Name,
        string Description,
        decimal Latitude,
        decimal Longitude,
        int NumberVolunteers,
        decimal Encouragement,
        Guid Images = default);

    public class GetTaskNamesByIdsRequest
    {
        public List<Guid> Ids { get; set; } = [];
    }
}
