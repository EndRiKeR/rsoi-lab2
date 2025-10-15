using BonusService.Database.ContextConfigurations;
using BonusService.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace BonusService.Database;

public class PrivilegeContext : DbContext
{
    public PrivilegeContext(DbContextOptions<PrivilegeContext> options) : base(options) { }

    public DbSet<Privilege> Privileges { get; set; }
    public DbSet<PrivilegeHistory> PrivilegeHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new PrivilegeConfiguration());
        modelBuilder.ApplyConfiguration(new PrivilegeHistoryConfiguration());
    }
}