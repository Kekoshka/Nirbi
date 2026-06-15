using ExceptionHandler.Exceptions;
using Microsoft.EntityFrameworkCore;
using MinorTaskService.DataAccess.Postgres.Context;
using MinorTaskService.DataAccess.Postgres.Models;
using MinorTaskService.WebApi.Common.DTO;
using MinorTaskService.WebApi.Common.Enums;
using MinorTaskService.WebApi.Interfaces;

namespace MinorTaskService.WebApi.Services
{
    public class TaskParticipantService : ITaskParticipantService
    {
        AppDbContext _context;
        ICurrentUserService _currentUserService;

        public TaskParticipantService(
            AppDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        // Список участников задачи. Доступ: только создатель задачи или сам участник.
        public async Task<List<Guid>> GetMinorTaskParticipants(Guid minorTaskId)
        {
            var currentUserId = _currentUserService.GetUserId();

            var task = await _context.MinorTasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == minorTaskId);
            if (task is null)
                throw new NotFoundException($"Minor task with id {minorTaskId} not found");

            var participants = await _context.TaskParticipants
                .Where(p => p.MinorTaskId == minorTaskId)
                .AsNoTracking()
                .Select(p => p.UserId)
                .ToListAsync();

            var isOwner = task.ConsumerId == currentUserId;
            var isParticipant = participants.Contains(currentUserId);
            if (!isOwner && !isParticipant)
                throw new ForbiddenException("Only the task owner or a participant can view participants");

            return participants;
        }

        // Добавление участника. Вызывается из confirmation-flow (через Gateway) при
        // принятии отклика. Здесь же — автопереход статуса при наборе нужного числа.
        public async Task AddTaskParticipantAsync(Guid minorTaskId, Guid userId, CancellationToken cancellationToken)
        {
            var task = await _context.MinorTasks
                .FirstOrDefaultAsync(t => t.Id == minorTaskId, cancellationToken);
            if (task is null)
                throw new NotFoundException($"Minor task with id {minorTaskId} not found");

            // Нельзя набирать волонтёров в задачу, которая уже не в поиске
            if (task.StatusId != StatusType.InSearch)
                throw new BadRequestException("Task is not open for volunteers");

            // Защита от дублей участника
            var already = await _context.TaskParticipants
                .AnyAsync(p => p.MinorTaskId == minorTaskId && p.UserId == userId, cancellationToken);
            if (already)
                return;

            TaskParticipant taskParticipant = new(minorTaskId, userId);
            await _context.TaskParticipants.AddAsync(taskParticipant, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Сколько уже набрано
            var count = await _context.TaskParticipants
                .CountAsync(p => p.MinorTaskId == minorTaskId, cancellationToken);

            // Набрали нужное количество → переводим в InProgress (выполняется),
            // дальше отклики на эту задачу не принимаются (проверка выше).
            if (count >= task.NumberVolunteers && task.StatusId == StatusType.InSearch)
            {
                task.UpdateStatus(StatusType.InProgress);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        // Исключение участника. Доступ: создатель задачи (любого) или сам участник (себя).
        public async Task RemoveTaskParticipantAsync(Guid minorTaskId, Guid participantId, CancellationToken cancellationToken)
        {
            var currentUserId = _currentUserService.GetUserId();

            var task = await _context.MinorTasks
                .FirstOrDefaultAsync(t => t.Id == minorTaskId, cancellationToken);
            if (task is null)
                throw new NotFoundException($"Minor task with id {minorTaskId} not found");

            var isOwner = task.ConsumerId == currentUserId;
            var isSelf = participantId == currentUserId;
            if (!isOwner && !isSelf)
                throw new ForbiddenException("Only the task owner or the participant themselves can remove a participant");

            var taskParticipant = await _context.TaskParticipants
                .FirstOrDefaultAsync(p => p.MinorTaskId == minorTaskId && p.UserId == participantId, cancellationToken);
            if (taskParticipant is null)
                throw new NotFoundException($"Task participant with id {participantId} in minor task with id {minorTaskId} not found");

            taskParticipant.Remove();
            await _context.SaveChangesAsync(cancellationToken);

            // Если задача была в работе и после исключения участников стало меньше нормы —
            // возвращаем в поиск, чтобы можно было добрать.
            var count = await _context.TaskParticipants
                .CountAsync(p => p.MinorTaskId == minorTaskId, cancellationToken);
            if (task.StatusId == StatusType.InProgress && count < task.NumberVolunteers)
            {
                task.UpdateStatus(StatusType.InSearch);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}