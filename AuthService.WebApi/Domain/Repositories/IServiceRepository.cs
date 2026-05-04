using AuthService.WebApi.Domain.Entities;

namespace AuthService.WebApi.Domain.Repositories
{
    public interface IServiceRepository
    {
        Task<ServiceEntity> GetByIdAsync(string serviceId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ServiceEntity>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(ServiceEntity service, CancellationToken cancellationToken = default);
        Task UpdateAsync(ServiceEntity service, CancellationToken cancellationToken = default);
        Task DeleteAsync(string serviceId, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string serviceId, CancellationToken cancellationToken = default);
    }
}
