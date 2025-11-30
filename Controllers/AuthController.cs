using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using CajaSanmiguel; 

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly CajaSanmiguelDbContext _context;

        public AuthController(IConfiguration config, CajaSanmiguelDbContext context)
        {
            _config = config;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            //Buscar usuario en la BD por Correo
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == login.Correo);

            // Validar usuario y contraseña
            if (usuario == null || usuario.Password != login.Password) 
            {
                return Unauthorized("Credenciales incorrectas");
            }

            //Crear Claims (Información del usuario dentro del token)
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol ?? "Empleado"), // Valor por defecto si es nulo
                new Claim(ClaimTypes.Name, usuario.Nombre)
            };

            //Generar la Clave de Seguridad y las Credenciales
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //Crear el Token JWT
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                //expires: DateTime.Now.AddHours(8)  AQUI LO COMENTE PARA LAS PRUEBAS
                signingCredentials: credentials); //Aquí usamos la variable 'credentials' definida arriba

            //Escribir el token como string
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { token = jwt });
        }
    }

    // DTO para el login
    public class LoginDto
    {
        public string Correo { get; set; }
        public string Password { get; set; }
    }
}