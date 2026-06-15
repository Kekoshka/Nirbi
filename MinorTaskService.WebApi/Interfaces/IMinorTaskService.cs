using ExceptionHandler.Exceptions;
using MinorTaskService.DataAccess.Postgres.Context;
using MinorTaskService.DataAccess.Postgres.Models;
using MinorTaskService.WebApi.Common.DTO;
using MinorTaskService.WebApi.Common.Enums;

namespace MinorTaskService.WebApi.Interfaces
{
    public interface IMinorTaskService
    {
        Task<Guid> CreateMinorTaskAsync(CreateMinorTaskDTO createDto, CancellationToken cancellationToken);
        Task<GetMinorTaskDTO> GetMinorTaskByIdAsync(Guid minorTaskId);
        Task<List<GetMinorTasksDTO>> GetMinorTasksFirstAsync(int limit, CancellationToken cancellationToken);
        Task<List<GetMinorTasksDTO>> GetMinorTasksBetweenAsync(int from, int to, CancellationToken cancellationToken);
        Task UpdateMinorTaskAsync(UpdateMinorTaskDTO updateDto, CancellationToken cancellationToken);
        Task UpdateMinorTaskStatusAsync(Guid minorTaskId, Guid statusId, CancellationToken cancellationToken);
        Task DeleteMinorTaskAsync(Guid minorTaskId, CancellationToken cancellationToken);
        Task<List<TaskCollectionDTO>> GetTaskCollectionsByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
        Task<PagedMinorTasksDTO> GetMinorTasksPagedAsync(int offset, int limit, string? search, string? status, string? sort, CancellationToken cancellationToken);
    }
}
