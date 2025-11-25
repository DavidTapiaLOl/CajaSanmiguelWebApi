
using CajaSanmiguel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        //..........Creacion de variable vinculada al contexto de la base de datos..........
        public readonly CajaSanmiguelDbContext _context;
        //..................................................................................


        //......Constructor donde se inicializa la variable _context .......................
        public UsuarioController(CajaSanmiguelDbContext context)
        {
            _context = context;
        }
        //...................................................................................


        //..........METODO PARA CREAR UN NUEVO USUAIRO......................................
        [HttpPost]
        public IActionResult AgregarUsuario([FromBody] Usuario usuario)
        {
           /*  // 🔎 EXTRAER el ID del usuario del JWT (ClaimTypes.NameIdentifier)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId))
            {
                // Esto no debería suceder si [Authorize] funciona correctamente
                return Unauthorized("ID de usuario no encontrado en el token.");
            } */
            
            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            return Ok($"Usuario agregado con exito");
        }
        //.................................................................................


        //..................PARA OBTENER LA LISTA DE USAURIOS..............................
        [HttpGet]
        public IActionResult ListaDeUsuarios()
        {
            return Ok(_context.Usuarios.ToList());
        }
        //.................................................................................

    }
}
