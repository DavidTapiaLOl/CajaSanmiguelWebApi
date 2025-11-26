using Microsoft.EntityFrameworkCore; 
using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.IdentityModel.Tokens; 
using System.Text; 
using FluentValidation;
using FluentValidation.AspNetCore;
using CajaSanmiguel; 

var builder = WebApplication.CreateBuilder(args);

// ========================================================================
// 1. CONFIGURACIÓN DE SERVICIOS (Inyección de Dependencias)
// ========================================================================

// A. Base de Datos (Primero lo esencial)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CajaSanmiguelDbContext>(options =>
    options.UseSqlServer(connectionString));

// B. Controladores y Configuración JSON (Para evitar ciclos infinitos)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

    // -- REGISTRAR FLUENT VALIDATION ---
builder.Services.AddFluentValidationAutoValidation(); // Habilita la validación automática en los controladores
builder.Services.AddFluentValidationClientsideAdapters(); // Opcional, útil para MVC clásico
builder.Services.AddValidatorsFromAssemblyContaining<Program>(); // Busca todos los validadores en tu proyecto
// --------------------------------------

// C. Seguridad (JWT)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// D. Documentación API (Swagger/OpenAPI)
builder.Services.AddOpenApi();


builder.Services.AddCors(options =>
{
    options.AddPolicy("NuevaPolitica", app =>
    {
        app.AllowAnyOrigin()
           .AllowAnyHeader()
           .AllowAnyMethod();
    });
});


// ========================================================================
// 2. CONSTRUCCIÓN DE LA APP
// ========================================================================
var app = builder.Build();

app.UseCors("NuevaPolitica");

// ========================================================================
// 3. CONFIGURACIÓN DEL PIPELINE (Middlewares - EL ORDEN ES CRÍTICO)
// ========================================================================

// A. Entorno de Desarrollo
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Swagger UI
}

// B. Redirección HTTPS
app.UseHttpsRedirection();

// C. SEGURIDAD (Orden: ¿Quién eres? -> ¿Qué puedes hacer?)
app.UseAuthentication(); 
app.UseAuthorization();

// D. Mapeo de Endpoints
app.MapControllers();

// ========================================================================
// 4. EJECUCIÓN
// ========================================================================
app.Run();