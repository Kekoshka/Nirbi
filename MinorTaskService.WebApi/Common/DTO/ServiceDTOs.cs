namespace MinorTaskService.WebApi.Common.DTO
{
    public record class CreateMinorTaskDTO(
        string Name,
        string Description,
        decimal Latitude,
        decimal Longitude,
        int NumberVolunteers,
        decimal Encouragement,
        Guid FileCollectionId);

    public record class UpdateMinorTaskDTO(
        Guid Id,
        string Name,
        string Description,
        decimal Latitude,
        decimal Longitude,
        int NumberVolunteers,
        decimal Encouragement);

    public record class GetMinorTaskDTO(
        Guid Id,
        string Name,
        string Description,
        decimal Latitude,
        decimal Longitude,
        int NumberVolunteers,
        decimal Encouragement,
        string Status,
        Guid ConsumerId,
        DateTime CreatedAt,
        Guid FileCollectionId);

    public record class GetMinorTasksDTO(
        Guid Id,
        string Name,
        string Description,
        decimal Latitude,
        decimal Longitude,
        int NumberVolunteers,   
        decimal Encouragement,
        string Status,          
        Guid ConsumerId,        
        DateTime CreatedAt,
        Guid FileCollectionId);

    public record class GetStatusesDTO(
        Guid Id,
        string Name);

    public record class GetMinorTaskParticipantsDTO(Guid UserId);

    public class PagedMinorTasksDTO
    {
        public int Total { get; set; }
        public List<GetMinorTasksDTO> Items { get; set; } = [];
    }

}
