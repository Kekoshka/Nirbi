using ConfirmationService.DataAccess.Models;
using ConfirmationService.WebApi.Common.DTO;
using ConfirmationService.WebApi.Common.DTO.ServiceDTO;

namespace ConfirmationService.WebApi.Common.Mappers
{
    public static class ConfirmationMapper
    {
        public static Confirmation ToConfirmation(this CreateConfirmationRequest value, Guid initiatorId)
        {
            var metaData = value.MetaData != null
                ? System.Text.Json.JsonSerializer.Serialize(value.MetaData)
                : "{}";

            var expiresAt = DateTime.UtcNow.AddHours(value.ExpirationHours);

            return new Confirmation(
                value.ConfirmationType,
                value.EntityId,
                initiatorId,         
                value.ReviewerId,
                metaData,
                expiresAt
            );
        }

        public static ConfirmationDTO ToConfirmationDTO(this Confirmation value) =>
            new ConfirmationDTO
            {
                Id = value.Id,
                ConfirmationType = value.ConfirmationType,
                Audits = value.Audits.Select(a => new ConfirmationAuditDTO
                {
                    Id = a.Id,
                    ChangedAt = a.ChangedAt,
                    ChangedBy = a.ChangedBy,
                    ConfirmationId = a.ConfirmationId,
                    NewStatus = a.NewStatus,
                    OldStatus = a.OldStatus
                }).ToList(),
                CreatedAt = value.CreatedAt,
                EntityId = value.EntityId,
                ExpiresAt = value.ExpiresAt,
                InitiatorId = value.InitiatorId,
                MetaData = value.MetaData,
                RejectionReason = value.RejectionReason,
                RespondedAt = value.RespondedAt,
                ReviewerId = value.ReviewerId,
                Status = value.Status

            };

        public static List<ConfirmationDTO> ToConfirmationsDTO(this List<Confirmation> value) =>
            value.Select(value => value.ToConfirmationDTO()).ToList();
    }
}