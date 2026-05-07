namespace ConfirmationService.WebApi.Common.DTO;

public class RespondToConfirmationRequest
{
    /// <summary>
    /// true = Accept, false = Reject
    /// </summary>
    public bool IsAccepted { get; set; }

    /// <summary>
    /// Причина отклонения (если IsAccepted = false)
    /// </summary>
    public string RejectionReason { get; set; }
}