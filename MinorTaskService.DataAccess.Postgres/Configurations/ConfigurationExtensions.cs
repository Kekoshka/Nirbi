using Microsoft.EntityFrameworkCore;
using MinorTaskService.DataAccess.Postgres.Models;
using System;
namespace MinorTaskService.DataAccess.Postgres.Configurations
{
    internal static class ConfigurationExtensions
    {
        public static void AddFilters(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MinorTask>().HasQueryFilter(mt => !mt.IsDeleted);
            modelBuilder.Entity<TaskParticipant>().HasQueryFilter(tp => tp.IsActive);
        }

        public static void ConfigureTaskParticipants(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskParticipant>().HasKey(x => new { x.MinorTaskId, x.UserId });
        }
    }
}
