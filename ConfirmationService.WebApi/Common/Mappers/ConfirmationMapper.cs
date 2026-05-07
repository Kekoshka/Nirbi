using ConfirmationService.DataAccess.Models;
using ConfirmationService.WebApi.Common.DTO;
using Riok.Mapperly.Abstractions;

namespace ConfirmationService.WebApi.Common.Mappers
{
    [Mapper]
    public static partial class ConfirmationMapper
    {
        public static partial Confirmation ToConfirmation(this CreateConfirmationRequest value);

    }
}
