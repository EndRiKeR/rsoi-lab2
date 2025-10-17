using FlightService.Database;
using FlightService.Database.Repositories;
using FlightService.Database.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<FlightContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

Console.WriteLine($"Connection string: {builder.Configuration.GetSection("DefaultConnection")}");

builder.Services.AddTransient<IAirportRepository, AirportRepository>();
builder.Services.AddTransient<IFlightRepository, FlightRepository>();

builder.Services.AddScoped<DatabaseFiller>();

var app = builder.Build();
var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<FlightContext>();
// var pendingMigrations = context.Database.GetPendingMigrations().ToList();
// if (pendingMigrations.Any())
// {
//     Console.WriteLine($"Applying {pendingMigrations.Count} migrations...");
//     context.Database.Migrate();
//     Console.WriteLine("Migrations applied successfully");
// }
// else
// {
//     Console.WriteLine("Database is up-to-date");
// }

Console.WriteLine($"[*][*][*]Before test data");
var filler = services.GetRequiredService<DatabaseFiller>();
await filler.AddTestData();
Console.WriteLine($"[*][*][*]After test data");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
