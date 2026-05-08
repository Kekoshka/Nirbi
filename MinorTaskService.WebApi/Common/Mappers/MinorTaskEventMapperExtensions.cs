using MinorTaskService.DataAccess.Postgres.DomainEvents.Events;
using Riok.Mapperly.Abstractions;
using Shared.Mapping;
using System.Numerics;

namespace MinorTaskService.Mapping;

public static class MinorTaskEventMapperExtensions
{
    public static MinorTaskService.MinorTaskCreated ToAvro(this MinorTaskCreatedEvent e) =>
        new()
        {
            MinorTaskId = e.MinorTaskId.ToString(),
            Name = e.Name,
            Description = e.Description,
            Latitude = e.Latitude.ToAvroDecimal(),
            Longitude = e.Longitude.ToAvroDecimal(),
            NumberVolunteers = e.NumberVolunteers,
            Encouragement = e.Encouragement.ToAvroDecimal(),
            StatusId = e.StatusId.ToString(),
            ConsumerId = e.ConsumerId.ToString(),
            CreatedAt = e.CreatedAt.ToUnixMillis(),
            EventId = e.EventId.ToString(),
            OccurredOn = e.OccurredOn.ToUnixMillis(),
        };

    public static MinorTaskService.MinorTaskDeleted ToAvro(this MinorTaskDeletedEvent e) =>
        new()
        {
            MinorTaskId = e.MinorTaskId.ToString(),
            EventId = e.EventId.ToString(),
            OccurredOn = e.OccurredOn.ToUnixMillis(),
        };

    public static MinorTaskService.MinorTaskStatusUpdated ToAvro(this MinorTaskStatusUpdatedEvent e) =>
        new()
        {
            MinorTaskId = e.MinorTaskId.ToString(),
            StatusId = e.StatusId.ToString(),
            EventId = e.EventId.ToString(),
            OccurredOn = e.OccurredOn.ToUnixMillis(),
        };

    public static MinorTaskService.MinorTaskUpdated ToAvro(this MinorTaskUpdatedEvent e) =>
        new()
        {
            MinorTaskId = e.MinorTaskId.ToString(),
            Name = e.Name,
            Description = e.Description,
            Latitude = e.Latitude.ToAvroDecimal(),
            Longitude = e.Longitude.ToAvroDecimal(),
            NumberVolunteers = e.NumberVolunteers,
            Encouragement = e.Encouragement.ToAvroDecimal(),
            EventId = e.EventId.ToString(),
            OccurredOn = e.OccurredOn.ToUnixMillis(),
        };

    public static MinorTaskService.TaskParticipantAdded ToAvro(this TaskParticipantAddedEvent e) =>
        new()
        {
            MinorTaskId = e.MinorTaskId.ToString(),
            UserId = e.UserId.ToString(),
            EventId = e.EventId.ToString(),
            OccurredOn = e.OccurredOn.ToUnixMillis(),
        };

    public static MinorTaskService.TaskParticipantRemoved ToAvro(this TaskParticipantRemovedEvent e) =>
        new()
        {
            MinorTaskId = e.MinorTaskId.ToString(),
            UserId = e.UserId.ToString(),
            EventId = e.EventId.ToString(),
            OccurredOn = e.OccurredOn.ToUnixMillis(),
        };
}