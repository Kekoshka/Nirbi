using NotificationService.WebApi.Common.AvroSchemas;
using Shared.Mapping;

namespace NotificationService.Mapping;

public static class AvroToEventMapperExtensions
{
    public static ConfirmationCreatedEvent ToEvent(this ConfirmationCreated avro) =>
        new(
            ConfirmationId: Guid.Parse(avro.ConfirmationId),
            ConfirmationType: avro.ConfirmationType,
            EntityId: Guid.Parse(avro.EntityId),
            InitiatorId: Guid.Parse(avro.InitiatorId),
            ReviewerId: Guid.Parse(avro.ReviewerId),
            Status: avro.Status,
            MetaData: avro.MetaData,
            CreatedAt: avro.CreatedAt.ToDateTime(),
            ExpiresAt: avro.ExpiresAt.ToDateTime()
        );

    public static ConfirmationRespondEvent ToEvent(this ConfirmationRespond avro) =>
        new(
            ConfirmationId: Guid.Parse(avro.ConfirmationId),
            InitiatorId: Guid.Parse(avro.InitiatorId),
            IsAccepted: avro.IsAccepted
        );

    public static ConfirmationRevokedEvent ToEvent(this ConfirmationRevoked avro) =>
        new(
            ConfirmationId: Guid.Parse(avro.ConfirmationId),
            ReviewerId: Guid.Parse(avro.ReviewerId)
        );

    public static MinorTaskCreatedEvent ToEvent(this MinorTaskCreated avro) =>
        new(
            MinorTaskId: Guid.Parse(avro.MinorTaskId),
            Name: avro.Name,
            Description: avro.Description,
            Latitude: avro.Latitude.ToDecimal(),
            Longitude: avro.Longitude.ToDecimal(),
            NumberVolunteers: avro.NumberVolunteers,
            Encouragement: avro.Encouragement.ToDecimal(),
            StatusId: Guid.Parse(avro.StatusId),
            ConsumerId: Guid.Parse(avro.ConsumerId),
            CreatedAt: avro.CreatedAt.ToDateTime()
        );

    public static MinorTaskDeletedEvent ToEvent(this MinorTaskDeleted avro) =>
        new(
            MinorTaskId: Guid.Parse(avro.MinorTaskId)
        );

    public static MinorTaskStatusUpdatedEvent ToEvent(this MinorTaskStatusUpdated avro) =>
        new(
            MinorTaskId: Guid.Parse(avro.MinorTaskId),
            StatusId: Guid.Parse(avro.StatusId)
        );

    public static MinorTaskUpdatedEvent ToEvent(this MinorTaskUpdated avro) =>
        new(
            MinorTaskId: Guid.Parse(avro.MinorTaskId),
            Name: avro.Name,
            Description: avro.Description,
            Latitude: avro.Latitude.ToDecimal(),
            Longitude: avro.Longitude.ToDecimal(),
            NumberVolunteers: avro.NumberVolunteers,
            Encouragement: avro.Encouragement.ToDecimal()
        );

    public static TaskParticipantAddedEvent ToEvent(this TaskParticipantAdded avro) =>
        new(
            MinorTaskId: Guid.Parse(avro.MinorTaskId),
            UserId: Guid.Parse(avro.UserId)
        );

    public static TaskParticipantRemovedEvent ToEvent(this TaskParticipantRemoved avro) =>
        new(
            MinorTaskId: Guid.Parse(avro.MinorTaskId),
            UserId: Guid.Parse(avro.UserId)
        );
}