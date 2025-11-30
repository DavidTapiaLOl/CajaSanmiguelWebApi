using CajaSanmiguel;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        public IActionResult AgregarCliente([FromBody] Cliente cliente)
        {
            _context.Clientes.Add(cliente);
            _context.SaveChanges();
            return Ok(cliente);
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

        //................Obtener Por parametros..............................
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarClientes(
            [FromQuery] string? nombre,
            [FromQuery] string? telefono,
            [FromQuery] string? direccion)
        {
            //muestra la tabla por defecto
            var query = _context.Clientes.AsQueryable();

            //filtro por Nombre o Apellido
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                query = query.Where(c => c.Nombre.Contains(nombre) || c.Apellidos.Contains(nombre));
            }

            //filtro por Teléfono
            if (!string.IsNullOrWhiteSpace(telefono))
            {
                query = query.Where(c => c.Telefono.Contains(telefono));
            }

            //filtro por Dirección
            if (!string.IsNullOrWhiteSpace(direccion))
            {
                query = query.Where(c => c.Direccion.Contains(direccion));
            }

            //se ejecuta la consulta y retornamos
            var resultados = await query.ToListAsync();
            return Ok(resultados);
        }
        //...................................................................

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
            return Ok(new { mensaje = "Cliente eliminado correctamente" });
        }
        //...................................................................

        [HttpPatch("{id}")]
        public async Task<IActionResult> ActualizarClienteManual(int id, [FromBody] ClienteUpdateDto clienteDto)
        {
            // se busca al cleinte por id
            var clienteDb = await _context.Clientes.FindAsync(id);

            if (clienteDb == null)
            {
                return NotFound($"No existe el cliente con ID {id}");
            }

            

            if (clienteDto.Nombre != null)
                clienteDb.Nombre = clienteDto.Nombre;

            if (clienteDto.Apellidos != null)
                clienteDb.Apellidos = clienteDto.Apellidos;

            if (clienteDto.Telefono != null)
                clienteDb.Telefono = clienteDto.Telefono;

            if (clienteDto.Direccion != null)
                clienteDb.Direccion = clienteDto.Direccion;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Ok(new { mensaje = "Cliente actualizado correctamente" });
        }



    }
}
