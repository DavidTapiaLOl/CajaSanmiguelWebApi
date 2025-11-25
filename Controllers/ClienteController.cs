using CajaSanmiguel;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        //.........variable vinculada al contexto de la base de datos........
        public readonly CajaSanmiguelDbContext _context;
        //...................................................................

        //..............Constructor para inicializar la variable.............
        public ClienteController(CajaSanmiguelDbContext context)
        {
            _context = context;
        }
        //...................................................................

        //................Metodo para agregar clientes.......................
        [HttpPost]
        public IActionResult AgregarCliente([FromBody]Cliente cliente)
        {
            _context.Clientes.Add(cliente);
            _context.SaveChanges();
            return Ok($"Cliente Agregado");
        }
        //...................................................................

        //..........OBTENER LA LISTA DE CLIENTES...............................
        [HttpGet]
        public IActionResult ObtenerListaClientes()
        {
            return Ok(_context.Clientes.Include(c => c.Prestamos).ToList());
        }
        //......................................................................
        //.................OBTENER USUARIO POR SI ID............................
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerCliente(int id)
        {
            var _cliente = await _context.Clientes.FindAsync(id);
            if (_cliente == null)
            {
                return NotFound("No se encontro el usuario");
            }
            return Ok(_cliente);
        }
        //.....................................................................

        //................Metodo para Eliminar clientes.......................
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCliente(int id)
        {
            var _cliente = await _context.Clientes.FindAsync(id);
            if (_cliente == null)
            {
                return NotFound($"No se encontró el cliente");
            }

            var tienePrestamos = await _context.Prestamos.AnyAsync(p => p.IdCliente == id);
            var _nombreCliente = _cliente.Nombre;
            var _apellidoCliente = _cliente.Apellidos;
            if (tienePrestamos)
            {
            // Retornamos 400 Bad Request con un mensaje explicativo
            return BadRequest($"No se puede eliminar al cliente {_nombreCliente} {_apellidoCliente} porque tiene préstamos registrados. Elimine los préstamos primero.");
            }
            _context.Clientes.Remove(_cliente);
            await _context.SaveChangesAsync();
            return Ok($"Cliente {_nombreCliente} {_apellidoCliente} eliminado");
        }
        //...................................................................

        [HttpPatch("{id}")]
public async Task<IActionResult> ActualizarClienteManual(int id, [FromBody] ClienteUpdateDto clienteDto)
{
    // 1. Buscar el cliente original en la Base de Datos
    var clienteDb = await _context.Clientes.FindAsync(id);

    if (clienteDb == null)
    {
        return NotFound($"No existe el cliente con ID {id}");
    }

    // 2. Actualización Manual (Mapeo Condicional)
    // Solo actualizamos si el valor NO es nulo (es decir, si enviaron algo nuevo)

    if (clienteDto.Nombre != null) 
        clienteDb.Nombre = clienteDto.Nombre;

    if (clienteDto.Apellidos != null) 
        clienteDb.Apellidos = clienteDto.Apellidos;

    if (clienteDto.Telefono != null) 
        clienteDb.Telefono = clienteDto.Telefono;

    if (clienteDto.Direccion != null) 
        clienteDb.Direccion = clienteDto.Direccion;

    // 3. Guardar los cambios
    // EF Core detecta automáticamente qué propiedades cambiaron.
    try 
    {
        await _context.SaveChangesAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
         // Manejo de errores de concurrencia (opcional pero recomendado)
         throw;
    }

    return Ok($"Datos actualizados");
}



    }
}
