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
    public class Pagocontroller : ControllerBase
    {
        public readonly CajaSanmiguelDbContext _context;
        public Pagocontroller(CajaSanmiguelDbContext context)
        {
            _context = context;
        }

        //.................LISTA DE PAGOS..........................
        [HttpGet]
        public async Task<IActionResult> ListaPagos()
        {
            // Agregamos AsNoTracking() para que sea más rápido si solo es lectura
            return Ok(await _context.Pagos.AsNoTracking().ToListAsync());
        }
        //........................................................

        //.................PETICION DE UN PAGO....................
        [HttpGet("{id}")]
        public async Task<IActionResult> ObetenerPago(int id)
        {
            var _pago = await _context.Pagos
                .Include(p => p.Prestamo) 
                .FirstOrDefaultAsync(p => p.IdPago == id);

            if (_pago == null)
            {
                return NotFound($"No se encontró el pago");
            }
            return Ok(_pago);
        }
        //.........................................................

        //..............................................................EDICION DE PAGOS........................................................
        [HttpPatch("{id}")]
        public async Task<IActionResult> EditarPago(int id, [FromBody] PagoUpdateDto pagoDto)
        {
            //..............VARIABLES...........................
            var _pago = await _context.Pagos
                .Include(p => p.Prestamo)
                .FirstOrDefaultAsync(p => p.IdPago == id);

            if (_pago == null) return NotFound("Pago no encontrado");

            //Cargar el préstamo CON sus pagos para poder recalcular
            var _infoPrestamo = await _context.Prestamos
                .Include(p => p.Pagos) //INCLUYE LA LISTA DE PAGOS
                .FirstOrDefaultAsync(x => x.IdPrestamo == _pago.IdPrestamo);

            if (_infoPrestamo == null) return NotFound("El préstamo asociado no existe");

            // Solo actualizamos la fecha real si se está registrando un pago (MontoPagado no es null)
            if (pagoDto.MontoPagado != null) 
            {
                 _pago.FechaPagoReal = DateTime.Now; 
            }
            //......................................................

            //..........EDICION FECHAPROGRAMADA..............
            if (pagoDto.FechaProgramada != null)
            {
                _pago.FechaProgramada = pagoDto.FechaProgramada.Value;
            }

            //.............BLOQUEOS DE EDICIÓN........................
            if (pagoDto.MontoProgramado != null) return BadRequest($"No se puede modificar el Monto Programado manualmente.");
            if (pagoDto.FechaPagoReal != null) return BadRequest($"No se puede modificar la Fecha Real manualmente.");

            //.......................EDICION MONTOPAGADO (Lógica Principal)............................
            if (pagoDto.MontoPagado != null)
            {
                _pago.MontoPagado = pagoDto.MontoPagado.Value;

                // VARIABLES AUXILIARES
                bool esPagoCompleto = _pago.MontoPagado >= _pago.MontoProgramado;
                bool esPuntual = _pago.FechaPagoReal.Value.Date <= _pago.FechaProgramada.Date;

                // LÓGICA DE ESTADOS
                if (esPagoCompleto && esPuntual)
                {
                    _pago.Estado = "Pagado";
                }
                else
                {
                    if (!esPuntual) // PAGO ATRASADO
                    {
                    
                        if (_pago.Estado != "Atrasado") //AQUI NO SE APLICA MULTA YA QUE EN EL PrestamoController YA ESTA LA LOGICA PARA QUE AUTOMATICAMENTE SE CAMBIE DE ESTADO Y NO APLIQUE LA MULTA
                        {
                            // Multa sobre el Total Global
                            _infoPrestamo.TotalAPagar = _infoPrestamo.TotalAPagar + (_infoPrestamo.TotalAPagar * _infoPrestamo.MontoMulta);
                        }

                        _pago.Estado = "Atrasado";

                        //RECALCULO DE CUOTAS
                        decimal dineroYaRecaudado = _infoPrestamo.Pagos
                            .Where(p => p.Estado == "Pagado")
                            .Sum(p => p.MontoPagado ?? p.MontoProgramado);

                        // Sumar lo actual
                        dineroYaRecaudado += _pago.MontoPagado.Value;

                        decimal saldoRestante = _infoPrestamo.TotalAPagar - dineroYaRecaudado;

                        // Contar cuotas pendientes EXCLUYENDO la actual (por ID) y las pagadas
                        int cuotasRestantes = _infoPrestamo.Pagos.Count(p => p.Estado != "Pagado" && p.IdPago != _pago.IdPago);

                        if (cuotasRestantes > 0)
                        {
                            decimal nuevoMontoPorCuota = saldoRestante / cuotasRestantes;

                            foreach (var pagoPendiente in _infoPrestamo.Pagos)
                            {
                                if (pagoPendiente.Estado != "Pagado" && pagoPendiente.IdPago != _pago.IdPago)
                                {
                                    pagoPendiente.MontoProgramado = nuevoMontoPorCuota;
                                }
                            }
                        }
                    }
                    else if (!esPagoCompleto)
                    {
                        _pago.Estado = "Pendiente"; // Pagó a tiempo, pero incompleto
                    }
                }
            }

            //.............EDICION MANUAL DE ESTADO....................
            if (pagoDto.Estado != null)
            {
                // Solo permitimos cambiar si no entra en conflicto con la lógica automática
                if (pagoDto.Estado == "Pagado")
                {
                    bool esPagoCompleto = _pago.MontoPagado >= _pago.MontoProgramado;
                    if (esPagoCompleto)
                    {
                        _pago.Estado = "Pagado";
                    }
                    else
                    {
                        return BadRequest("No puedes marcar como 'Pagado' si el monto no está completo.");
                    }
                }
                else 
                {
                     _pago.Estado = pagoDto.Estado;
                }
            }

            //...............GUARDAR CAMBIOS.............
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return Ok($"Datos guardados");
        }

        //......................BORRAR PAGOS.................
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarPago(int id)
        {
            var _pago = await _context.Pagos.FindAsync(id);
            
            if (_pago == null) return NotFound("No existe el pago");

            var _estadoPago = _pago.Estado;
            //no se puede borrar porque aun no esta pagado
            if (_estadoPago == "Pendiente" || _estadoPago == "Atrasado")
            {
                return BadRequest($"No se puede eliminar el pago porque tiene estado: {_estadoPago}");
            }

            _context.Pagos.Remove(_pago);
            await _context.SaveChangesAsync();
            return Ok($"Pago eliminado");
        }
    }
}