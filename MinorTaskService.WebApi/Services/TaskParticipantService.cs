using MinorTaskService.DataAccess.Postgres.Context;
using MinorTaskService.DataAccess.Postgres.Models;
using MinorTaskService.WebApi.Interfaces;

namespace MinorTaskService.WebApi.Services
{
    public class TaskParticipantService : ITaskParticipantService
    {
        AppDbContext _context;
        public TaskParticipantService(AppDbContext context) 
        {
            _context = context;
        }

        public async Task AddTaskParticipantAsync(Guid minorTaskId, Guid userId, CancellationToken cancellationToken)
        {
            TaskParticipant taskParticipant = new(minorTaskId, userId);

            await _context.TaskParticipants.AddAsync(taskParticipant, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveTaskParticipantAsync(Guid minorTaskId, Guid participantId, CancellationToken cancellationToken)
        {
            var taskParticipant = await _context.TaskParticipants.FindAsync(minorTaskId, participantId, cancellationToken);
            if (taskParticipant is null)
                throw new DirectoryNotFoundException($"Task participant with id {participantId} in minor task with id {minorTaskId} not found");
        
            taskParticipant.Remove();
            await _context.SaveChangesAsync(cancellationToken);
        }

    }
}
