using CommunicationService.DataAccess.Postgres.DomainEvents;
using Shared.Mapping;

namespace CommunicationService.WebApi.Common.Mappers
{
    public static class AvroMappers
    {
        public static CommunicationService.ChatCreated ToAvro(this ChatCreatedEvent e) =>
            new()
            {
                Id = e.Id.ToString(),
                Name = e.Name,
                ChatTypeId = e.ChatTypeId.ToString(),
                ChatUsers = e.ChatUsers.Select(u => u).ToList(),
                EventId = e.EventId.ToString(),
                OccurredOn = e.OccurredOn.ToUnixMillis(),
            };

        public static CommunicationService.MessageCreated ToAvro(this MessageCreatedEvent e) =>
            new()
            {
                Id = e.Id.ToString(),
                Sender = e.Sender.ToString(),
                ChatId = e.ChatId.ToString(),
                Content = e.Content,
                CreatedAt = e.CreatedAt.ToUnixMillis(),
                ChatUsers = e.ChatUsers.Select(u => u).ToList(),
                EventId = e.EventId.ToString(),
                OccurredOn = e.OccurredOn.ToUnixMillis(),
            };

        public static CommunicationService.MessageDeleted ToAvro(this MessageDeletedEvent e) =>
            new()
            {
                Id = e.Id.ToString(),
                ChatId = e.ChatId.ToString(),
                ChatUsers = e.ChatUsers.Select(u => u).ToList(),
                EventId = e.EventId.ToString(),
                OccurredOn = e.OccurredOn.ToUnixMillis(),
            };

        public static CommunicationService.MessageUpdated ToAvro(this MessageUpdatedEvent e) =>
            new()
            {
                Id = e.Id.ToString(),
                ChatId = e.ChatId.ToString(),
                Content = e.Content,
                ChatUsers = e.ChatUsers.Select(u => u).ToList(),
                EventId = e.EventId.ToString(),
                OccurredOn = e.OccurredOn.ToUnixMillis(),
            };

        public static CommunicationService.UserJoined ToAvro(this UserJoinedEvent e) =>
            new()
            {
                UserId = e.UserId.ToString(),
                ChatId = e.ChatId.ToString(),
                ChatUsers = e.ChatUsers.Select(u => u).ToList(),
                EventId = e.EventId.ToString(),
                OccurredOn = e.OccurredOn.ToUnixMillis(),
            };

        public static CommunicationService.UserRemoved ToAvro(this UserRemovedEvent e) =>
            new()
            {
                UserId = e.UserId.ToString(),
                ChatId = e.ChatId.ToString(),
                ChatUsers = e.ChatUsers.Select(u => u).ToList(),
                EventId = e.EventId.ToString(),
                OccurredOn = e.OccurredOn.ToUnixMillis(),
            };

    }
}
