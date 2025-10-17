using BonusService.Database.ContextConfigurations;
using BonusService.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace BonusService.Database;

public class PrivilegeContext : DbContext
{
    public DbSet<Privilege> Privileges { get; set; }
    public DbSet<PrivilegeHistory> PrivilegeHistories { get; set; }
    
    public PrivilegeContext() { }
    public PrivilegeContext(DbContextOptions<PrivilegeContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new PrivilegeConfiguration());
        modelBuilder.ApplyConfiguration(new PrivilegeHistoryConfiguration());
    }
}