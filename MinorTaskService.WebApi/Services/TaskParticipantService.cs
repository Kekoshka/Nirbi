using ExceptionHandler.Exceptions;
using Microsoft.EntityFrameworkCore;
using MinorTaskService.DataAccess.Postgres.Context;
using MinorTaskService.DataAccess.Postgres.Models;
using MinorTaskService.WebApi.Common.DTO;
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

        public async Task<List<Guid>> GetMinorTaskParticipants(Guid minorTaskId)
        {
            var participants = await _context.TaskParticipants
                .Where(p => p.MinorTaskId == minorTaskId)
                .AsNoTracking()
                .Select(p => p.UserId) 
                .ToListAsync();

            return participants;
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
                throw new NotFoundException($"Task participant with id {participantId} in minor task with id {minorTaskId} not found");
        
            taskParticipant.Remove();
            await _context.SaveChangesAsync(cancellationToken);
        }

    }
}
