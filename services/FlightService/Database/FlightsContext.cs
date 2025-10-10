using Microsoft.EntityFrameworkCore;
using RsoiLab2.Services.Tickets.Database.Models;

namespace Tickets.Database;

public class FlightContext : DbContext
{
    public FlightContext(DbContextOptions<FlightContext> options) : base(options) { }

    public DbSet<Airport> Airports { get; set; }
    public DbSet<Flight> Flights { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new FlightsConfiguration());
        modelBuilder.ApplyConfiguration(new AirportsConfiguration());
    }
}