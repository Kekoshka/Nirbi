using AuthService.WebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace AuthService.WebApi.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        public DbSet<ServiceEntity> Services { get; set; }
        public DbSet<ServiceScopeEntity> ServiceScopes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ServiceEntity>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<ServiceEntity>()
                .HasMany(s => s.AllowedScopes)
                .WithOne(sc => sc.Service)
                .HasForeignKey(sc => sc.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServiceScopeEntity>()
                .HasKey(ss => ss.Id);

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Опционально: сид данные при создании БД
            // modelBuilder.Entity<ServiceEntity>().HasData(
            //     new ServiceEntity { Id = "service-orders", Name = "Orders Service", ... }
            // );
        }
    }
}
