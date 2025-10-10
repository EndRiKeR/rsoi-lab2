using Microsoft.EntityFrameworkCore;
using RsoiLab2.Services.Tickets.Database.Models;

namespace Tickets.Database;

public class TicketsContext : DbContext
{
    public virtual DbSet<Ticket> Tickets { get; set; }

    public TicketsContext() { }
    public TicketsContext(DbContextOptions<TicketsContext> dbContextOptions) : base(dbContextOptions) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new TicketsConfiguration());
    }
}
