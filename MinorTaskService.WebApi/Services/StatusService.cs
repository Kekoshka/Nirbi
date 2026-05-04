using Microsoft.EntityFrameworkCore;
using MinorTaskService.DataAccess.Postgres.Context;
using MinorTaskService.WebApi.Common.DTO;
using MinorTaskService.WebApi.Common.Mappers;
using MinorTaskService.WebApi.Interfaces;

namespace MinorTaskService.WebApi.Services
{
    public class StatusService : IStatusService
    {
        AppDbContext _context;
        public StatusService(
            AppDbContext context) 
        {
            _context = context;
        }

        public async Task<List<GetStatusesDTO>> GetStatusesAsync(CancellationToken cancellationToken)
        {
            return await _context.Statuses
                .AsQueryable()
                .ToGetStatusesDTO()
                .ToListAsync(cancellationToken);
        }
    }
}
