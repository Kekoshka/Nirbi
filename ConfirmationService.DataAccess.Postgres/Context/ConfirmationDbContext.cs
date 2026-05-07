using ConfirmationService.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace ConfirmationService.DataAccess.Context;

public class ConfirmationDbContext : DbContext
{
    public ConfirmationDbContext(DbContextOptions<ConfirmationDbContext> options) : base(options)
    {
    }

    public DbSet<Confirmation> Confirmations { get; set; }
    public DbSet<ConfirmationAudit> ConfirmationAudits { get; set; }
}