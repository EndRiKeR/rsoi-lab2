using BonusService.Database;
using BonusService.Database.Models;
using BonusService.Database.Repositories;
using Common.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PrivilegeContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddTransient<IRepository<Privilege>, PrivilegeRepository>();
builder.Services.AddTransient<IRepository<PrivilegeHistory>, PrivilegeHistoryRepository>();

var app = builder.Build();

var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var context = services.GetRequiredService<PrivilegeContext>();
var pendingMigrations = context.Database.GetPendingMigrations().ToList();
if (pendingMigrations.Any())
{
    Console.WriteLine($"Applying {pendingMigrations.Count} migrations...");
    context.Database.Migrate();
    Console.WriteLine("Migrations applied successfully");
}
else
{
    Console.WriteLine("Database is up-to-date");
}

// var initDatabaseJob = services.GetRequiredService<InitializeDatabaseJob>();
// await initDatabaseJob.InitializeDatabaseAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
