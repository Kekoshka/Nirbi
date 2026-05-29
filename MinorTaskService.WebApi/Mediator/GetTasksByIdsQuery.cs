using MediatR;
using Microsoft.EntityFrameworkCore;
using MinorTaskService.DataAccess.Postgres.Context;
using MinorTaskService.WebApi.Common.DTO;

namespace MinorTaskService.WebApi.Mediator;

/// <summary>Получить пары Id+Name для списка задач. Используется Gateway для агрегации.</summary>
public record GetTasksByIdsQuery(List<Guid> Ids) : IRequest<List<TaskNameResponse>>;

public class GetTasksByIdsQueryHandler : IRequestHandler<GetTasksByIdsQuery, List<TaskNameResponse>>
{
    private readonly AppDbContext _context;

    public GetTasksByIdsQueryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TaskNameResponse>> Handle(
        GetTasksByIdsQuery request, CancellationToken cancellationToken)
    {
        return await _context.MinorTasks
            .Where(t => request.Ids.Contains(t.Id))
            .Select(t => new TaskNameResponse(t.Id, t.Name))
            .ToListAsync(cancellationToken);
    }
}
