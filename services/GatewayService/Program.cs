var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddHttpClient("FlightService", client =>
{
    client.BaseAddress = new Uri("http://localhost:8060");
});

builder.Services.AddHttpClient("TicketsService", client =>
{
    client.BaseAddress = new Uri("http://localhost:8070");
});

builder.Services.AddHttpClient("BonusService", client =>
{
    client.BaseAddress = new Uri("http://localhost:8050");
});

var app = builder.Build();

app.UseRouting();
app.MapControllers();

app.Run();
