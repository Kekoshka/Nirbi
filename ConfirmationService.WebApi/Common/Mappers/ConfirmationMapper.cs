using ConfirmationService.DataAccess.Models;
using ConfirmationService.WebApi.Common.DTO;

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
    }
}