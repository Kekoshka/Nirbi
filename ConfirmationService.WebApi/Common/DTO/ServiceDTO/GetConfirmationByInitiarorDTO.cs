using ConfirmationService.DataAccess.Models;

namespace ConfirmationService.WebApi.Common.DTO.ServiceDTO
{
    public class ConfirmationDTO
    {
        public Guid Id { get; set; }
        public string ConfirmationType { get; set; }
        public Guid EntityId { get; set; }
        public Guid InitiatorId { get; set; }
        public Guid ReviewerId { get; set; }
        public string Status { get; set; }
        public string MetaData { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? RespondedAt { get; set; }
        public string? RejectionReason { get; set; }
        public ICollection<ConfirmationAuditDTO> Audits { get; set; } = new List<ConfirmationAuditDTO>();
    }

    public class ConfirmationAuditDTO()
    {
        public Guid Id { get; set; }
        public Guid ConfirmationId { get; set; }
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public Guid ChangedBy { get; set; }
    }
}
