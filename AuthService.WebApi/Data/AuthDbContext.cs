using AuthService.WebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace AuthService.WebApi.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ServiceEntity> Services { get; set; }
        public DbSet<ServiceScopeEntity> ServiceScopes { get; set; }
    }
}
