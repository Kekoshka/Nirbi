namespace ConfirmationService.WebApi.Common.DTO.ServiceDTO
{
    public record class RespondToConfirmationDTO(
        Guid ConfirmationId,
        bool IsAccepted,
        string RejectionReason);
}
