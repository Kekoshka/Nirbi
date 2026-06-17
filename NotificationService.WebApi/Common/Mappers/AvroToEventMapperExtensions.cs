using NotificationService.WebApi.Common.AvroSchemas;
using NotificationService.WebApi.Common.Events;
using Shared.Mapping;
using AvroChatCreated = NotificationService.ChatCreated;
using AvroMessageCreated = NotificationService.MessageCreated;
using AvroMessageDeleted = NotificationService.MessageDeleted;
using AvroMessageUpdated = NotificationService.MessageUpdated;
using AvroUserJoined = NotificationService.UserJoined;
using AvroUserRemoved = NotificationService.UserRemoved;

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

    public static ChatCreatedEvent ToEvent(this AvroChatCreated avro) =>
        new(
            Id: Guid.Parse(avro.Id),
            Name: avro.Name,
            ChatTypeId: Guid.Parse(avro.ChatTypeId),
            ChatUsers: avro.ChatUsers?.ToList() ?? []
        );

    public static MessageCreatedEvent ToEvent(this AvroMessageCreated avro) =>
        new(
            Id: Guid.Parse(avro.Id),
            Sender: Guid.Parse(avro.Sender),
            ChatId: Guid.Parse(avro.ChatId),
            Content: avro.Content,
            CreatedAt: avro.CreatedAt.ToDateTime(),
            ChatUsers: avro.ChatUsers?.ToList() ?? []
        );

    public static MessageDeletedEvent ToEvent(this AvroMessageDeleted avro) =>
        new(
            Id: Guid.Parse(avro.Id),
            ChatId: Guid.Parse(avro.ChatId),
            ChatUsers: avro.ChatUsers?.ToList() ?? []
        );

    public static MessageUpdatedEvent ToEvent(this AvroMessageUpdated avro) =>
        new(
            Id: Guid.Parse(avro.Id),
            ChatId: Guid.Parse(avro.ChatId),
            Content: avro.Content,
            ChatUsers: avro.ChatUsers?.ToList() ?? []
        );

    public static UserJoinedEvent ToEvent(this AvroUserJoined avro) =>
        new(
            UserId: Guid.Parse(avro.UserId),
            ChatId: Guid.Parse(avro.ChatId),
            ChatUsers: avro.ChatUsers?.ToList() ?? []
        );

    public static UserRemovedEvent ToEvent(this AvroUserRemoved avro) =>
        new(
            UserId: Guid.Parse(avro.UserId),
            ChatId: Guid.Parse(avro.ChatId),
            ChatUsers: avro.ChatUsers?.ToList() ?? []
        );
}