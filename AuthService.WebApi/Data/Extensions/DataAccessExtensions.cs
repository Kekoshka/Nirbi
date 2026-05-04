using AuthService.WebApi.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AuthService.WebApi.Data.Extensions
{
    public static class DataAccessExtensions
    {
        public static IServiceCollection AddDataAccess(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<AuthDbContext>(options =>
                options.UseNpgsql(connectionString)
            );

            services.AddScoped<IServiceRepository, ServiceRepository>();

            return services;
        }

        public static async Task ApplyMigrationsAsync(
            this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
                await dbContext.Database.MigrateAsync();
            }
        }
    }

}
