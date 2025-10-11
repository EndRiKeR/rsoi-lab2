using Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using TicketsService.Database;
using TicketsService.Database.Models;
using TicketsService.Database.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddTransient<IRepository<Ticket>, TicketRepository>();

builder.Services.AddDbContext<TicketsContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

app.UseRouting();
app.MapControllers();

app.Run();