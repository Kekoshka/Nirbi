using ExceptionHandler.Exceptions;
using Microsoft.EntityFrameworkCore;
using MinorTaskService.DataAccess.Postgres.Context;
using MinorTaskService.DataAccess.Postgres.Models;
using MinorTaskService.WebApi.Common.DTO;
using MinorTaskService.WebApi.Common.Enums;
using MinorTaskService.WebApi.Common.Mappers;
using MinorTaskService.WebApi.Interfaces;

namespace MinorTaskService.WebApi.Services
{
    public class MinorTaskService : IMinorTaskService
    {
        AppDbContext _context;
        ICurrentUserService _currentUserService;

        public MinorTaskService(
            AppDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Guid> CreateMinorTaskAsync(CreateMinorTaskDTO createDto, CancellationToken cancellationToken)
        {
            var minorTask = new MinorTask(
                createDto.Name,
                createDto.Description,
                createDto.Latitude,
                createDto.Longitude,
                createDto.NumberVolunteers,
                createDto.Encouragement,
                StatusType.InSearch,
                _currentUserService.GetUserId(),
                createDto.FileCollectionId);

            await _context.MinorTasks.AddAsync(minorTask, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return minorTask.Id;
        }

        public async Task<GetMinorTaskDTO> GetMinorTaskByIdAsync(Guid minorTaskId)
        {
            var minorTask = await _context.MinorTasks.Include(mt => mt.Status).FirstOrDefaultAsync(mt => mt.Id == minorTaskId);
            if (minorTask is null)
                throw new NotFoundException($"Minor task with id {minorTaskId} not found");

            return minorTask.ToGetMinorTaskDTO();
        }

        public async Task<List<GetMinorTasksDTO>> GetMinorTasksFirstAsync(int limit, CancellationToken cancellationToken)
        {
            var minorTasks = await _context.MinorTasks
                .Where(mt => mt.StatusId == StatusType.InSearch)
                .OrderBy(mt => mt.Id)
                .ToGetMinorTasksDTO()
                .Take(limit)
                .ToListAsync(cancellationToken);

            if (minorTasks is null)
                throw new NotFoundException("Minor tasks not found");

            return minorTasks;
        }

        public async Task<List<GetMinorTasksDTO>> GetMinorTasksBetweenAsync(int from, int to, CancellationToken cancellationToken)
        {
            var minorTasks = await _context.MinorTasks
                .Include(mt => mt.Status)
                .Where(mt => mt.StatusId == StatusType.InSearch)
                .OrderBy(mt => mt.Id)
                .ToGetMinorTasksDTO()
                .Skip(from)
                .Take(to)
                .ToListAsync(cancellationToken);

            if(minorTasks is null)
                throw new NotFoundException($"Minor tasks from {from} to {to} not found");

            return minorTasks;
        }

        public async Task UpdateMinorTaskAsync(UpdateMinorTaskDTO updateDto, CancellationToken cancellationToken)
        {
            var minorTask = await _context.MinorTasks.FindAsync(updateDto.Id, cancellationToken);
            if (minorTask is null)
                throw new NotFoundException($"Minor task with id {updateDto.Id} not found");

            minorTask.Update(
                updateDto.Name,
                updateDto.Description,
                updateDto.Latitude,
                updateDto.Longitude,
                updateDto.NumberVolunteers, 
                updateDto.Encouragement);
            await _context.SaveChangesAsync(cancellationToken);
        }
        
        public async Task UpdateMinorTaskStatusAsync(Guid minorTaskId, Guid statusId, CancellationToken cancellationToken)
        {
            var minorTask = await _context.MinorTasks.FindAsync(minorTaskId,cancellationToken);
            if (minorTask is null)
                throw new NotFoundException($"Minor task with id {minorTaskId} not found");

            minorTask.UpdateStatus(statusId);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteMinorTaskAsync(Guid minorTaskId, CancellationToken cancellationToken)
        {
            var minorTask = await _context.MinorTasks.FindAsync(minorTaskId, cancellationToken);
            if (minorTask is null)
                throw new NotFoundException($"Minor task with id {minorTaskId} not found");

            minorTask.Delete();
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
