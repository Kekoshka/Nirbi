using DataService.DataAccess.Postgres.Models;
using Microsoft.EntityFrameworkCore;

namespace DataService.DataAccess.Postgres.Context;

public class DataDbContext : DbContext
{
    public DataDbContext(DbContextOptions<DataDbContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<FileCollection> FileCollections => Set<FileCollection>();
    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileCollection>(e =>
        {
            e.ToTable("file_collections");
            e.HasKey(x => x.Id);
            e.Property(x => x.CreatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<StoredFile>(e =>
        {
            e.ToTable("stored_files");
            e.HasKey(x => x.Id);
            e.Property(x => x.StorageKey).IsRequired().HasMaxLength(512);
            e.HasIndex(x => x.StorageKey).IsUnique();
            e.Property(x => x.ContentType).IsRequired().HasMaxLength(256);
            e.Property(x => x.OriginalFileName).HasMaxLength(512);
            e.Property(x => x.CreatedAtUtc).IsRequired();

            e.HasOne(x => x.FileCollection)
                .WithMany(c => c.Files)
                .HasForeignKey(x => x.FileCollectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
