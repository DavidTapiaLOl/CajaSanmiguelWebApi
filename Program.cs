using Microsoft.EntityFrameworkCore; // <-- Soluciona el error de 'UseSqlServer' (CS1061)
using CajaSanmiguel;


var builder = WebApplication.CreateBuilder(args);
// ...





builder.Services.AddOpenApi();

// Obtener la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Registrar el DbContext con SQL Server
builder.Services.AddDbContext<CajaSanmiguelDbContext>(options =>
    options.UseSqlServer(connectionString));








builder.Services.AddControllers();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
