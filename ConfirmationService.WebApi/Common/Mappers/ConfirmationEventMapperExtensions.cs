using ConfirmationService.DataAccess.Postgres.DomainEvents;
using Shared.Mapping;
namespace ConfirmationService.Mapping;

public static class ConfirmationEventMapperExtensions
{
    public static ConfirmationService.ConfirmationCreated ToAvro(this ConfirmationCreatedEvent e) =>
        new()
        {
            ConfirmationId = e.ConfirmationId.ToString(),
            ConfirmationType = e.ConfirmationType,
            EntityId = e.EntityId.ToString(),
            InitiatorId = e.InitiatorId.ToString(),
            ReviewerId = e.ReviewerId.ToString(),
            Status = e.Status,
            MetaData = e.MetaData,
            CreatedAt = e.CreatedAt.ToUnixMillis(),
            ExpiresAt = e.ExpiresAt.ToUnixMillis(),
            EventId = e.EventId.ToString(),
            OccurredOn = e.OccurredOn.ToUnixMillis(),
        };

    public static ConfirmationService.ConfirmationRespond ToAvro(this ConfirmationRespondEvent e) =>
        new()
        {
            ReviewerId = e.ReviewerId.ToString(),
            ConfirmationType = e.ConfirmationType,
            EntityId = e.EntityId.ToString(),
            ConfirmationId = e.ConfirmationId.ToString(),
            InitiatorId = e.InitiatorId.ToString(),
            IsAccepted = e.IsAccepted,
            EventId = e.EventId.ToString(),
            OccurredOn = e.OccurredOn.ToUnixMillis(),
        };

    public static ConfirmationService.ConfirmationRevoked ToAvro(this ConfirmationRevokedEvent e) =>
        new()
        {
            ConfirmationId = e.ConfirmationId.ToString(),
            ReviewerId = e.ReviewerId.ToString(),
            EventId = e.EventId.ToString(),
            OccurredOn = e.OccurredOn.ToUnixMillis(),
        };
}