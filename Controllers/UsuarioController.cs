
using CajaSanmiguel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
     [Authorize]
    public class UsuarioController : ControllerBase
    {
        
        public readonly CajaSanmiguelDbContext _context;

        public UsuarioController(CajaSanmiguelDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult AgregarUsuario([FromBody] Usuario usuario)
        {
            // 🔎 EXTRAER el ID del usuario del JWT (ClaimTypes.NameIdentifier)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                // Esto no debería suceder si [Authorize] funciona correctamente
                return Unauthorized("ID de usuario no encontrado en el token.");
            }
            
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            return Ok($"Usuario agregado con exito");
        }


        
        [HttpGet]
        public IActionResult ListaDeUsuarios()
        {
            return Ok(_context.Usuarios.ToList());
        }

    }
}
