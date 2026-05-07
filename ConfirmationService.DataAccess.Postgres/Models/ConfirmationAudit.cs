namespace ConfirmationService.DataAccess.Models;

public class ConfirmationAudit
{
    public Guid Id { get; set; }

    public Guid ConfirmationId { get; set; }

    public string OldStatus { get; set; }

    public string NewStatus { get; set; }

    public DateTime ChangedAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// ID пользователя, который совершил действие
    /// </summary>
    public Guid ChangedBy { get; set; }

    public Confirmation Confirmation { get; set; }

    public ConfirmationAudit(Guid confirmationId, Guid changedBy, string newStatus, string oldStatus = "")
    {
        Id = Guid.NewGuid();
        ConfirmationId = confirmationId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangedBy = changedBy;
    }
}