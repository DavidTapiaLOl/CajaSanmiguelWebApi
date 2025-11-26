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

        // =================================================================================
        // 1. CREAR PRÉSTAMO (POST)
        // Genera el calendario de pagos inicial automáticamente.
        // =================================================================================
        [HttpPost]
        public async Task<IActionResult> CrearPrestamo([FromBody] Prestamo prestamo)
        {
            // Calcular Totales Iniciales
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

        // =================================================================================
        // 2. LISTA DE PRÉSTAMOS (GET)
        // Revisa vencimientos masivamente y guarda cambios una sola vez (Optimizado).
        // =================================================================================
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
                // Pasamos 'false' para NO guardar en cada iteración (ahorra recursos)
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

        // =================================================================================
        // 3. OBTENER UN PRÉSTAMO (GET ID)
        // Revisa vencimientos y guarda inmediatamente para mostrar datos al día.
        // =================================================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPrestamo(int id)
        {
            var prestamo = await _context.Prestamos
                .Include(p => p.Pagos)
                .FirstOrDefaultAsync(p => p.IdPrestamo == id);

            if (prestamo == null) return NotFound("No se encontró el préstamo");

            // Aquí sí guardamos inmediatamente (true) para asegurar consistencia al ver el detalle
            await VerificarEstadoPrestamo(prestamo, true);

            return Ok(prestamo);
        }

        // =================================================================================
        // 4. ELIMINAR PRÉSTAMO (DELETE)
        // Solo permite eliminar si no está Activo (o según tu regla de negocio).
        // =================================================================================
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
            return Ok($"Préstamo eliminado");
        }

        // =================================================================================
        // 5. ACTUALIZAR PRÉSTAMO (PATCH) - Lógica Maestra
        // Maneja cambios de dinero (recalcular) y cambios de tiempo (regenerar pagos).
        // =================================================================================
        [HttpPatch("{id}")]
        public async Task<IActionResult> ActualizarPrestamo(int id, [FromBody] PrestamoUpdateDto prestamoDto)
        {
            // 1. Validación básica manual (reemplaza al validador)
            if (prestamoDto.Monto <= 0) return BadRequest("El monto debe ser positivo.");
            if (prestamoDto.NumeroCuotas <= 0) return BadRequest("Debe haber al menos una cuota.");
            
            // 1. Traer el préstamo con sus pagos
            var _prestamo = await _context.Prestamos
                .Include(p => p.Pagos) 
                .FirstOrDefaultAsync(p => p.IdPrestamo == id);

            if (_prestamo == null) return NotFound($"No se encontró el préstamo");

            // Banderas de control
            bool recalcularMontos = false;     // Para cambios de dinero (Monto/Interés)
            bool reestructurarPlazo = false;   // Para cambios de tiempo (Fecha/Cuotas/Lapso)

            // --- Detección de Cambios Estructurales (Tiempo) ---
            if (prestamoDto.FechaInicio != null || prestamoDto.NumeroCuotas != null || prestamoDto.Lapzo != null)
            {
                reestructurarPlazo = true;
            }

            // 🛡️ VALIDACIÓN DE SEGURIDAD 🛡️
            // Si quiere cambiar la estructura, verificamos que NO haya empezado a pagar.
            if (reestructurarPlazo)
            {
                bool yaHayPagos = _prestamo.Pagos.Any(p => p.Estado == "Pagado" || p.Estado == "Atrasado");
                if (yaHayPagos)
                {
                    return BadRequest("No se puede modificar el Plazo, Fecha o #Cuotas porque el préstamo ya tiene pagos registrados. Se requiere una refinanciación.");
                }
            }

            // --- Actualización de Propiedades Simples ---
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
                        return BadRequest("No se puede cambiar el estado a 'Terminado' porque existen cuotas pendientes.");
                    }
                }
                _prestamo.Estado = prestamoDto.Estado;
            }

            // --- Actualización de Propiedades que Afectan Cálculos ---
            
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


            // =================================================================================
            // ESCENARIO 1: REESTRUCTURACIÓN TOTAL (Cambió Fecha, Cuotas o Lapso)
            // =================================================================================
            if (reestructurarPlazo)
            {
                // 1. Borrar los pagos viejos de la base de datos
                _context.Pagos.RemoveRange(_prestamo.Pagos);
                
                // 2. Recalcular el Total Global (por si también cambió monto/interés)
                _prestamo.TotalAPagar = _prestamo.Monto + (_prestamo.Monto * _prestamo.Interes);
                
                // 3. Preparar variables para el bucle
                DateTime fechaCalculo = _prestamo.FechaInicio;
                decimal montoPorCuota = _prestamo.TotalAPagar / _prestamo.NumeroCuotas;
                
                // 4. Regenerar la lista nueva
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
            // =================================================================================
            // ESCENARIO 2: SOLO CAMBIO DE MONTOS (Mantiene las fechas originales)
            // =================================================================================
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

            return Ok($"Datos actualizados correctamente.");
        }

        // ========================================================================
        // MÉTODO PRIVADO: CEREBRO DEL LAZY UPDATE (Multas Automáticas)
        // ========================================================================
        private async Task<bool> VerificarEstadoPrestamo(Prestamo prestamo, bool autoGuardar = true)
        {
            bool huboCambios = false;
            DateTime fechaHoy = DateTime.Now;
            string estadoMora = "Atrasado"; 

            // 1. DETECCIÓN DE ATRASOS
            foreach (var pago in prestamo.Pagos.Where(p => p.Estado != "Pagado"))
            {
                // Si venció ayer o antes y no tiene la etiqueta de mora
                if (pago.FechaProgramada.Date < fechaHoy.Date && pago.Estado != estadoMora)
                {
                    pago.Estado = estadoMora;
                    huboCambios = true;

                    // APLICAR MULTA: Aumentamos el Total Global un X% (ej. 4%)
                    prestamo.TotalAPagar = prestamo.TotalAPagar + (prestamo.TotalAPagar * prestamo.MontoMulta);
                }
            }

            // 2. REDISTRIBUCIÓN DE LA NUEVA DEUDA
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

                // Guardar si se solicitó (True para GET individual, False para listas masivas)
                if (autoGuardar)
                {
                    await _context.SaveChangesAsync();
                }
            }

            return huboCambios;
        }
    }
}