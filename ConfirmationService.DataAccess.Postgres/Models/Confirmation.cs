using ConfirmationService.DataAccess.Enums;
using ConfirmationService.DataAccess.Postgres.DomainEvents;
using ConfirmationService.DataAccess.Postgres.DomainEvents.Interfaces;
using ConfirmationService.WebApi.DomainEvents.Events;

namespace ConfirmationService.DataAccess.Models;

public class Confirmation : IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();


    public Guid Id { get; set; }

    /// <summary>
    /// Тип подтверждения: "TaskJoin", "UserAdd", "TaskCompletion" и т.д.
    /// </summary>
    public string ConfirmationType { get; set; }

    /// <summary>
    /// ID сущности (TaskId, UserId, и т.д.)
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// ID инициатора подтверждения (кто требует присоединиться/действие)
    /// </summary>
    public Guid InitiatorId { get; set; }

    /// <summary>
    /// ID пользователя, который должен одобрить (создатель задачи, администратор и т.д.)
    /// </summary>
    public Guid ReviewerId { get; set; }

    /// <summary>
    /// Статус: Created, Pending, Accepted, Rejected, Expired, Revoked
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Дополнительные данные в JSON формате для различных типов подтверждений
    /// </summary>
    public string MetaData { get; set; }

    /// <summary>
    /// Дата создания
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата истечения подтверждения
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Дата ответа (принято/отклонено)
    /// </summary>
    public DateTime? RespondedAt { get; set; }

    /// <summary>
    /// Причина отклонения
    /// </summary>
    public string RejectionReason { get; set; }

    /// <summary>
    /// История изменений
    /// </summary>
    public ICollection<ConfirmationAudit> Audits { get; set; } = new List<ConfirmationAudit>();

    public Confirmation(string confirmationType, Guid entityId, Guid initiatorId, Guid reviewerId, string metaData, DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        ConfirmationType = confirmationType;
        EntityId = entityId;
        InitiatorId = initiatorId;
        ReviewerId = reviewerId;
        Status = ConfirmationStatus.Created.ToString();
        MetaData = metaData;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;

        var audit = new ConfirmationAudit(
            Id, 
            InitiatorId,
            ConfirmationStatus.Created.ToString());
        Audits.Add(audit);

        _domainEvents.Add(new ConfirmationCreatedEvent(Id, ConfirmationType, EntityId, InitiatorId, ReviewerId, Status, MetaData, CreatedAt, ExpiresAt));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    public void Respond(bool isAccepted, Guid initiatorId, string rejectionReason = "")
    {
        var oldStatus = Status;

        if (isAccepted)
        {
            Status = ConfirmationStatus.Accepted.ToString();
            RespondedAt = DateTime.UtcNow;

            ConfirmationAudit confirmationAudit = new(
                Id,
                initiatorId,
                ConfirmationStatus.Accepted.ToString(),
                oldStatus);

            Audits.Add(confirmationAudit);
        }
        else
        {
            Status = ConfirmationStatus.Rejected.ToString();
            RejectionReason = rejectionReason;
            RespondedAt = DateTime.UtcNow;

            ConfirmationAudit confirmationAudit = new(
                Id,
                initiatorId,
                ConfirmationStatus.Rejected.ToString(),
                oldStatus);

            Audits.Add(confirmationAudit);
        }

        _domainEvents.Add(new ConfirmationRespondEvent(Id, InitiatorId, isAccepted));
    }

    public void Revoke(Guid initiatorId)
    {
        var oldStatus = Status;
        Status = ConfirmationStatus.Revoked.ToString();
        RespondedAt = DateTime.UtcNow;

        ConfirmationAudit confirmationAudit = new(
            Id,
            initiatorId,
            ConfirmationStatus.Revoked.ToString(),
            oldStatus);
        Audits.Add(confirmationAudit);

        _domainEvents.Add(new ConfirmationRevokedEvent(Id, ReviewerId));
    }


}