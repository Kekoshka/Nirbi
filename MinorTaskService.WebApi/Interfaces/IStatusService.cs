using MinorTaskService.WebApi.Common.DTO;

namespace MinorTaskService.WebApi.Interfaces
{
    public interface IStatusService
    {
        Task<List<GetStatusesDTO>> GetStatusesAsync(CancellationToken cancellationToken);
    }
}
