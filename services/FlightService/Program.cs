using Common.Interfaces;
using FlightService.Database;
using FlightService.Database.Models;
using FlightService.Database.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddTransient<IRepository<Airport>, AirportRepository>();
builder.Services.AddTransient<IRepository<Flight>, FlightRepository>();

builder.Services.AddDbContext<FlightContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

app.UseRouting();
app.MapControllers();

app.Run();