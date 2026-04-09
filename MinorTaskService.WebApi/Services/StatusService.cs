using Microsoft.EntityFrameworkCore;
using MinorTaskService.DataAccess.Postgres.Context;
using MinorTaskService.WebApi.Common.DTO;
using MinorTaskService.WebApi.Common.Mappers;

namespace MinorTaskService.WebApi.Services
{
    public class StatusService
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
