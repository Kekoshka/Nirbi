namespace MinorTaskService.WebApi.Common.DTO
{
    public record class GetMinorTaskResponse(
        Guid Id,                 
        string Name,
        string Description,
        decimal Latitude,
        decimal Longitude,
        int NumberVolunteers,
        decimal Encouragement,
        string Status,
        string ConsumerId,
        DateTime CreatedAt,
        Guid FileCollectionId);

    public record class GetMinorTasksResponse(
        Guid Id,
        string Name,
        string Description,
        decimal Latitude,
        decimal Longitude,
        int NumberVolunteers,      
        decimal Encouragement,     
        string Status,             
        string ConsumerId,         
        DateTime CreatedAt,
        Guid FileCollectionId);

    public record class GetStatusesResponse(
        Guid Id,
        string Name);

    public record TaskNameResponse(
        Guid Id, 
        string Name);

}