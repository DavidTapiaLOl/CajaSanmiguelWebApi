using CajaSanmiguel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MyApp.Namespace
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PrestamoController : ControllerBase
    {
        public readonly CajaSanmiguelDbContext _context;
        public PrestamoController(CajaSanmiguelDbContext context)
        {
            _context = context;
        }


        //.....................CREAR PRESTAMO.......................................
        [HttpPost]
        public async Task<IActionResult> CrearPrestamo([FromBody] Prestamo prestamo)
        {
            //vARIABLES QUE SE NECESITAN
            prestamo.FechaInicio = DateTime.Now;
            prestamo.TotalAPagar = prestamo.Monto + (prestamo.Monto * prestamo.Interes);
            DateTime fechaCalculo = prestamo.FechaInicio;
            decimal montoPorCuota = prestamo.TotalAPagar / prestamo.NumeroCuotas;


            // Generar la lista de pagos
            for (int i = 1; i <= prestamo.NumeroCuotas; i++)
            {
                if (prestamo.Lapzo == "Semanal") fechaCalculo = fechaCalculo.AddDays(7);
                else if (prestamo.Lapzo == "Quincenal") fechaCalculo = fechaCalculo.AddDays(15);
                else if (prestamo.Lapzo == "Mensual") fechaCalculo = fechaCalculo.AddMonths(1);

                //Objeto para pagos
                var nuevoPago = new Pago
                {
                    NumeroCuota = i,
                    FechaProgramada = fechaCalculo,
                    MontoProgramado = montoPorCuota,
                    Estado = "Pendiente",
                    FechaPagoReal = null
                };
                prestamo.Pagos.Add(nuevoPago);
            }
            _context.Prestamos.Add(prestamo);
            await _context.SaveChangesAsync();
            return Ok(prestamo);
        }
        //.......................................................................

        //.......................Obtener prestamos...............................
        // Revisa vencimientos masivamente y guarda cambios.
        [HttpGet]
        public async Task<IActionResult> ListaPrestamos()
        {
            var lista = await _context.Prestamos
                .Include(p => p.Pagos)
                .Include(p => p.Cliente)
                .ToListAsync();

            bool algunCambioGlobal = false;

            // Revisamos cada préstamo para ver si hay vencimientos hoy
            foreach (var p in lista)
            {
                bool cambioEnEste = await VerificarEstadoPrestamo(p, false);
                if (cambioEnEste) algunCambioGlobal = true;
            }

            // Si hubo multas nuevas, guardamos todo de una sola vez
            if (algunCambioGlobal)
            {
                await _context.SaveChangesAsync();
            }

            return Ok(lista);
        }
        //......................................................................

        //....................Obtener prestamo por id...........................
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPrestamo(int id)
        {
            var prestamo = await _context.Prestamos
                .Include(p => p.Pagos)
                .Include(p => p.Cliente)
                .FirstOrDefaultAsync(p => p.IdPrestamo == id);

            if (prestamo == null) return NotFound("No se encontró el préstamo");
            //se revisa si hay cambios
            await VerificarEstadoPrestamo(prestamo, true);

            return Ok(prestamo);
        }
        //....................................................................

        //.................PRESTAMO POR CLIENTE................................
        [HttpGet("cliente/{idCliente}")]
        public async Task<IActionResult> GetPrestamosPorCliente(int idCliente)
        {
            var prestamos = await _context.Prestamos
                .Where(p => p.IdCliente == idCliente)
                .Include(p => p.Pagos)
                .OrderByDescending(p => p.IdPrestamo)
                .ToListAsync();

            // Si las lista esta vacia devolvemos un 200 pero NotFund
            if (prestamos == null)
            {
                return Ok(new List<Prestamo>());
            }

            //Recorremos la lista para verificar CADA préstamo
            bool huboCambiosGlobales = false;

            //Recorre toda la lista de prestamos 
            foreach (var unPrestamo in prestamos)
            {
                //revisa cambios
                bool cambio = await VerificarEstadoPrestamo(unPrestamo, false);
                if (cambio) huboCambiosGlobales = true;
            }

            //Si hubo cambios en alguno, guardamos todo junto al final
            if (huboCambiosGlobales)
            {
                await _context.SaveChangesAsync();
            }

            return Ok(prestamos);
        }
        //.................................................................................

        //.....................BUSCAR PRESTAMO POR PARAMETROS..............................
        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarPrestamos(
            [FromQuery] string? cliente,
            [FromQuery] decimal? monto,
            [FromQuery] string? lapzo,
            [FromQuery] int? cuotas,
            [FromQuery] string? estado)
        {
            //TABLA POR DEFAULT
            var query = _context.Prestamos
                .Include(p => p.Cliente) //INFORMACION DEL CLIENTE
                .Include(p => p.Pagos)   //INFORMACION DE PAGOS
                .AsQueryable();

            //filtro por nombre de clientE
            if (!string.IsNullOrWhiteSpace(cliente))
            {
                query = query.Where(p => p.Cliente.Nombre.Contains(cliente) || p.Cliente.Apellidos.Contains(cliente));
            }

            //filtro por Monto
            if (monto.HasValue)
            {
                query = query.Where(p => p.Monto == monto.Value);
            }

            //filtro por lapzo, va ignorar si dice todos
            if (!string.IsNullOrWhiteSpace(lapzo) && lapzo != "Todos")
            {
                query = query.Where(p => p.Lapzo == lapzo);
            }

            //filtro por nmero de cuotas
            if (cuotas.HasValue)
            {
                query = query.Where(p => p.NumeroCuotas == cuotas.Value);
            }

            //filtro por estado ignorando el todos
            if (!string.IsNullOrWhiteSpace(estado) && estado != "Todos")
            {
                //Convertimos ambos lados a minúsculas y quitamos espacios para asegurar coincidencia
                string estadoLimpio = estado.Trim().ToLower();

                query = query.Where(p => p.Estado.ToLower() == estadoLimpio);
            }

            //se ejecuta la consulta
            var resultados = await query
                .OrderByDescending(p => p.IdPrestamo) // Los más recientes primero
                .ToListAsync();

            return Ok(resultados);
        }
        //..................................................................................

       //.............................ELIMINAR PRESTAMO.....................................
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarPrestamo(int id)
        {
            var _prestamo = await _context.Prestamos.FindAsync(id);

            if (_prestamo == null) return NotFound("Préstamo no encontrado");

            if (_prestamo.Estado.Equals("Activo"))
            {
                return BadRequest($"No se puede eliminar un préstamo que está Activo.");
            }

            _context.Prestamos.Remove(_prestamo);
            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Datos guardados correctamente" });
        }

        //..........................MODIFICAR PRESTAMO....................................
        // Maneja cambios de dinero (recalcular) y cambios de tiempo (regenerar pagos)
        [HttpPatch("{id}")]
        public async Task<IActionResult> ActualizarPrestamo(int id, [FromBody] PrestamoUpdateDto prestamoDto)
        {
            //Validaciones manuales
            if (prestamoDto.Monto <= 0) return BadRequest("El monto debe ser positivo.");
            if (prestamoDto.NumeroCuotas <= 0) return BadRequest("Debe haber al menos una cuota.");

            // Traer el préstamo con sus pagos
            var _prestamo = await _context.Prestamos
                .Include(p => p.Pagos)
                .FirstOrDefaultAsync(p => p.IdPrestamo == id);

            if (_prestamo == null) return NotFound($"No se encontró el préstamo");


            bool recalcularMontos = false;     // Para cambios de dinero (Monto/Interés)
            bool reestructurarPlazo = false;   // Para cambios de tiempo (Fecha/Cuotas/Lapso)

            //SI HAY CAMBIOS EN ALGUNO DE ESTOS 3, SE ACTIVA LA LOGICA
            if ((prestamoDto.FechaInicio != null && prestamoDto.FechaInicio != _prestamo.FechaInicio) ||
                (prestamoDto.NumeroCuotas != null && prestamoDto.NumeroCuotas != _prestamo.NumeroCuotas) ||
                (prestamoDto.Lapzo != null && prestamoDto.Lapzo != _prestamo.Lapzo))
            {
                reestructurarPlazo = true;
            }

            // Si ya hay pagos registrados, no se puede restrucutrar
            if (reestructurarPlazo)
            {
                bool yaHayPagos = _prestamo.Pagos.Any(p => p.Estado == "Pagado" || p.Estado == "Atrasado");
                if (yaHayPagos)
                {
                    return BadRequest(new { message = "No se puede modificar el Plazo, Fecha o #Cuotas porque el préstamo ya tiene pagos registrados. Se requiere una refinanciación." });
                }
            }

            
            if (prestamoDto.IdCliente != null) _prestamo.IdCliente = prestamoDto.IdCliente.Value;
            if (prestamoDto.MontoMulta != null) _prestamo.MontoMulta = prestamoDto.MontoMulta.Value;

            // Validación de Estado "Terminado"
            if (prestamoDto.Estado != null)
            {
                if (prestamoDto.Estado == "Terminado")
                {
                    bool hayDeudas = await _context.Pagos
                        .AnyAsync(p => p.IdPrestamo == id && p.Estado != "Pagado");

                    if (hayDeudas)
                    {
                        return BadRequest(new { message = "No se puede cambiar el estado a 'Terminado' porque existen cuotas pendientes de pago." });
                    }
                }
                _prestamo.Estado = prestamoDto.Estado;
            }


            // Campos de Dinero
            if (prestamoDto.Monto != null)
            {
                _prestamo.Monto = prestamoDto.Monto.Value;
                recalcularMontos = true;
            }
            if (prestamoDto.Interes != null)
            {
                _prestamo.Interes = prestamoDto.Interes.Value;
                recalcularMontos = true;
            }

            // Campos de Tiempo
            if (prestamoDto.FechaInicio != null) _prestamo.FechaInicio = prestamoDto.FechaInicio.Value;
            if (prestamoDto.NumeroCuotas != null) _prestamo.NumeroCuotas = prestamoDto.NumeroCuotas.Value;
            if (prestamoDto.Lapzo != null) _prestamo.Lapzo = prestamoDto.Lapzo;


            //restructuracion, cambio de fecha,lapzo o  num cuotas
            if (reestructurarPlazo)
            {
                //primero se borran los pagos existentes
                _context.Pagos.RemoveRange(_prestamo.Pagos);

                //Recalcular el Total Global (por si también cambió monto/interés)
                _prestamo.TotalAPagar = _prestamo.Monto + (_prestamo.Monto * _prestamo.Interes);

                //variables para el bucle
                DateTime fechaCalculo = _prestamo.FechaInicio;
                decimal montoPorCuota = _prestamo.TotalAPagar / _prestamo.NumeroCuotas;

                //Regenerar la lista nueva
                for (int i = 1; i <= _prestamo.NumeroCuotas; i++)
                {
                    if (_prestamo.Lapzo == "Semanal") fechaCalculo = fechaCalculo.AddDays(7);
                    else if (_prestamo.Lapzo == "Quincenal") fechaCalculo = fechaCalculo.AddDays(15);
                    else if (_prestamo.Lapzo == "Mensual") fechaCalculo = fechaCalculo.AddMonths(1);
                    var nuevoPago = new Pago
                    {
                        // IdPrestamo se asigna automático al agregarlo a la lista
                        NumeroCuota = i,
                        FechaProgramada = fechaCalculo,
                        MontoProgramado = montoPorCuota,
                        Estado = "Pendiente",
                        FechaPagoReal = null
                    };
                    _prestamo.Pagos.Add(nuevoPago);
                }
            }


            //Restructuracion de montos
            else if (recalcularMontos)
            {
                _prestamo.TotalAPagar = _prestamo.Monto + (_prestamo.Monto * _prestamo.Interes);

                decimal dineroYaPagado = _prestamo.Pagos
                    .Where(p => p.Estado == "Pagado")
                    .Sum(p => p.MontoPagado ?? p.MontoProgramado);

                decimal saldoRestante = _prestamo.TotalAPagar - dineroYaPagado;
                int cuotasPendientes = _prestamo.Pagos.Count(p => p.Estado != "Pagado");

                if (cuotasPendientes > 0)
                {
                    decimal nuevoMontoPorCuota = saldoRestante / cuotasPendientes;

                    foreach (var pago in _prestamo.Pagos.Where(p => p.Estado != "Pagado"))
                    {
                        pago.MontoProgramado = nuevoMontoPorCuota;
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Ok(new { mensaje = "Datos guardados correctamente" });
        }

       
       //metodo para la logica de pagos atrasados, se manda a llamar en el metodo get
        private async Task<bool> VerificarEstadoPrestamo(Prestamo prestamo, bool autoGuardar = true)
        {
            bool huboCambios = false;
            DateTime fechaHoy = DateTime.Now;
            string estadoMora = "Atrasado";

            //DETECCIÓN DE ATRASOS
            foreach (var pago in prestamo.Pagos.Where(p => p.Estado != "Pagado"))
            {
                // Si venció ayer o antes y no tiene la etiqueta de mora
                if (pago.FechaProgramada.Date < fechaHoy.Date && pago.Estado != estadoMora)
                {
                    pago.Estado = estadoMora;
                    huboCambios = true;

                    // se aplica multa sumandole el interes al total a pagar
                    prestamo.TotalAPagar = prestamo.TotalAPagar + (prestamo.TotalAPagar * prestamo.MontoMulta);
                }
            }

            // restructuracion de los pagos
            if (huboCambios)
            {
                decimal dineroYaPagado = prestamo.Pagos
                    .Where(p => p.Estado == "Pagado")
                    .Sum(p => p.MontoPagado ?? p.MontoProgramado);

                decimal saldoRestante = prestamo.TotalAPagar - dineroYaPagado;
                int cuotasPendientes = prestamo.Pagos.Count(p => p.Estado != "Pagado");

                if (cuotasPendientes > 0)
                {
                    decimal nuevoMontoPorCuota = saldoRestante / cuotasPendientes;

                    foreach (var pago in prestamo.Pagos.Where(p => p.Estado != "Pagado"))
                    {
                        pago.MontoProgramado = nuevoMontoPorCuota;
                    }
                }

                // Guardar si se solicitó
                if (autoGuardar)
                {
                    await _context.SaveChangesAsync();
                }
            }

            return huboCambios;
        }
    }
}