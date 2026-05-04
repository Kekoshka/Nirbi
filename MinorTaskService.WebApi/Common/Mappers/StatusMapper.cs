using MinorTaskService.DataAccess.Postgres.Models;
using MinorTaskService.WebApi.Common.DTO;
using Riok.Mapperly.Abstractions;

namespace MinorTaskService.WebApi.Common.Mappers
{
    [Mapper]
    public static partial class StatusMapper
    {
        public static partial IQueryable<GetStatusesDTO> ToGetStatusesDTO(this IQueryable<Status> value);

        public static partial List<GetStatusesResponse> ToGetStatusesResponse(this List<GetStatusesDTO> value);
    }
}
