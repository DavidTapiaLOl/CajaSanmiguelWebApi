
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
        public async Task<IActionResult> CrearUsuario([FromBody] UsuarioRegistroDto usuarioDto)
        {
            //Pasamos los datos del DTO al Modelo real (Mapeo)
            var nuevoUsuario = new Usuario
            {
                Nombre = usuarioDto.Nombre,
                Correo = usuarioDto.Correo,
                Password = usuarioDto.Password, // Aquí sí se lee la contraseña
                Rol = usuarioDto.Rol ?? "Empleado" // Rol por defecto si no envían nada
            };

            //Guardamos en la base de datos
            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return Ok(nuevoUsuario);
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
