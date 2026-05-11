using DataService.DataAccess.Postgres.Models;
using Microsoft.EntityFrameworkCore;

namespace DataService.DataAccess.Postgres.Context;

public class DataDbContext : DbContext
{
    public DataDbContext(DbContextOptions options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<FileCollection> FileCollections { get; set; }
    public DbSet<StoredFile> StoredFiles { get; set; }
    public DbSet<Owner> Owners { get; set; }
}
