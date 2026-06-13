namespace ConfirmationService.WebApi.Common.DTO;

public class ConfirmationResponse
{
    public Guid Id { get; set; }

    public string ConfirmationType { get; set; }

    public Guid EntityId { get; set; }

    public Guid InitiatorId { get; set; }

    public Guid ReviewerId { get; set; }

    public string Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RespondedAt { get; set; }

    public string? RejectionReason { get; set; }

    public Dictionary<string, object> MetaData { get; set; }
}