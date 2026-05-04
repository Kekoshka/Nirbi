using AuthService.WebApi.Data;
using AuthService.WebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.WebApi.Domain.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly AuthDbContext _dbContext;

        public ServiceRepository(AuthDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ServiceEntity> GetByIdAsync(string serviceId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Services
                .Include(s => s.AllowedScopes)
                .FirstOrDefaultAsync(s => s.Id == serviceId, cancellationToken);
        }

        public async Task<IEnumerable<ServiceEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Services
                .Include(s => s.AllowedScopes)
                .Where(s => s.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(ServiceEntity service, CancellationToken cancellationToken = default)
        {
            _dbContext.Services.Add(service);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(ServiceEntity service, CancellationToken cancellationToken = default)
        {
            _dbContext.Services.Update(service);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(string serviceId, CancellationToken cancellationToken = default)
        {
            var service = await GetByIdAsync(serviceId, cancellationToken);
            if (service != null)
            {
                _dbContext.Services.Remove(service);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<bool> ExistsAsync(string serviceId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Services.AnyAsync(s => s.Id == serviceId, cancellationToken);
        }
    }

}
