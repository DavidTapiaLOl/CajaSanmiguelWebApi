using Microsoft.EntityFrameworkCore; // <-- Soluciona el error de 'UseSqlServer' (CS1061)
using CajaSanmiguel;


var builder = WebApplication.CreateBuilder(args);
// ...


//.....PARA OBTENER LOS SERVICIOS DE LOS CONTROLADORES.....................
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
//........................................................................


builder.Services.AddOpenApi();

//..................Obtener la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Registrar el DbContext con SQL Server
builder.Services.AddDbContext<CajaSanmiguelDbContext>(options =>
    options.UseSqlServer(connectionString));
//..........................................................................










var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

//...Mepeo de los controlladores..
app.MapControllers();
//................................


app.Run();
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
