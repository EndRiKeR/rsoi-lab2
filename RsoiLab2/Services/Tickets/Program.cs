var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// builder.Services.AddTransient<IRepository<Person>, PersonRepository>();

builder.Services.AddDbContext<Context>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
