using BonusService.Database;
using BonusService.Database.Models;
using BonusService.Database.Repositories;
using Common.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddTransient<IRepository<Privilege>, PrivilegeRepository>();
builder.Services.AddTransient<IRepository<PrivilegeHistory>, PrivilegeHistoryRepository>();

builder.Services.AddDbContext<PrivilegeContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

app.UseRouting();
app.MapControllers();

app.Run();
