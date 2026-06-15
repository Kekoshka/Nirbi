using Microsoft.EntityFrameworkCore;
using MinorTaskService.DataAccess.Postgres.Models;

namespace MinorTaskService.WebApi.Interfaces
{
    public interface ITaskParticipantService
    {
        Task<List<Guid>> GetMinorTaskParticipants(Guid minorTaskId);

        Task AddTaskParticipantAsync(Guid minorTaskId, Guid userId, CancellationToken cancellationToken);

        Task RemoveTaskParticipantAsync(Guid minorTaskId, Guid participantId, CancellationToken cancellationToken);

    }
}
