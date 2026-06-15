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
            var minorTask = await _context.MinorTasks
                .Include(mt => mt.Status)
                .FirstOrDefaultAsync(mt => mt.Id == minorTaskId);
            if (minorTask is null)
                throw new NotFoundException($"Minor task with id {minorTaskId} not found");

            return minorTask.ToGetMinorTaskDTO();
        }

        public async Task<List<GetMinorTasksDTO>> GetMinorTasksFirstAsync(int limit, CancellationToken cancellationToken)
        {
            var minorTasks = await _context.MinorTasks
                .Include(mt => mt.Status)
                .Where(mt => mt.StatusId == StatusType.InSearch)
                .OrderBy(mt => mt.Id)
                .Take(limit)
                .ToListAsync();

            if (minorTasks is null)
                throw new NotFoundException("Minor tasks not found");

            return minorTasks.ToGetMinorTasksDTO();
        }

        public async Task<List<GetMinorTasksDTO>> GetMinorTasksBetweenAsync(int from, int to, CancellationToken cancellationToken)
        {
            var minorTasks = await _context.MinorTasks
                .Include(mt => mt.Status)
                .Where(mt => mt.StatusId == StatusType.InSearch)
                .OrderBy(mt => mt.Id)
                .Skip(from)
                .Take(to)
                .ToListAsync(cancellationToken);

            if(minorTasks is null)
                throw new NotFoundException($"Minor tasks from {from} to {to} not found");

            return minorTasks.ToGetMinorTasksDTO();
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

        public async Task<List<TaskCollectionDTO>> GetTaskCollectionsByIdsAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken)
        {
            if (ids is null || ids.Count == 0)
                return [];

            var idList = ids.Distinct().ToList();

            // Берём только id и коллекцию — без статуса/связей, запрос лёгкий.
            return await _context.MinorTasks
                .Where(mt => idList.Contains(mt.Id))
                .Select(mt => new TaskCollectionDTO
                {
                    Id = mt.Id,
                    FileCollectionId = mt.FileCollectionId
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedMinorTasksDTO> GetMinorTasksPagedAsync(
            int offset, int limit, string? search, string? status, string? sort,
            CancellationToken cancellationToken)
        {
            if (limit <= 0) limit = 20;
            if (limit > 100) limit = 100;
            if (offset < 0) offset = 0;

            IQueryable<MinorTask> query = _context.MinorTasks
                .Include(mt => mt.Status)
                .AsQueryable();

            // По умолчанию показывать только задачи «в поиске». Если нужен показ всех —
            // убери эту строку. Если фронт прислал конкретный статус — он переопределит ниже.
            query = query.Where(mt => mt.StatusId == StatusType.InSearch);

            // Поиск по названию (подстрока, регистронезависимо на уровне БД (ILIKE в PG))
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(mt => EF.Functions.ILike(mt.Name, $"%{term}%"));
            }

            // Фильтр по имени статуса (если фронт прислал)
            if (!string.IsNullOrWhiteSpace(status))
            {
                var st = status.Trim();
                query = query.Where(mt => mt.Status != null && mt.Status.Name == st);
            }

            // Сортировка
            query = sort switch
            {
                "reward" => query.OrderByDescending(mt => mt.Encouragement).ThenBy(mt => mt.Id),
                "volunteers" => query.OrderByDescending(mt => mt.NumberVolunteers).ThenBy(mt => mt.Id),
                // newest по умолчанию: если есть CreatedAt — лучше по нему; иначе по Id
                _ => query.OrderByDescending(mt => mt.Id),
            };

            var total = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip(offset)
                .Take(limit)
                .ToListAsync(cancellationToken);

            return new PagedMinorTasksDTO
            {
                Total = total,
                Items = items.ToGetMinorTasksDTO(),
            };
        }

    }
}
