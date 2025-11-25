using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CajaSanmiguel;

public class Pago
{
    [Key]
    public int IdPago { get; set; } // PK (Antes IdRegistroPago)

    // ✅ AQUÍ VA LA LLAVE FORÁNEA
    public int IdPrestamo { get; set; } 

    // Datos de la Programación (Lo que genera tu algoritmo)
    public int NumeroCuota { get; set; } // Ej: Cuota 1 de 10
    public DateTime FechaProgramada { get; set; } // Cuándo DEBE pagar
    public decimal MontoProgramado { get; set; } // Cuánto DEBE pagar

    // Datos del Pago Real (Se llenan cuando el cliente paga)
    public DateTime? FechaPagoReal { get; set; } // Puede ser nulo si no ha pagado
    public decimal? MontoPagado { get; set; }
    public string Estado { get; set; } // "Pendiente", "Pagado", "Atrasado"

    // --- RELACIÓN ---
    [ForeignKey("IdPrestamo")]
    [JsonIgnore]
    public Prestamo? Prestamo { get; set; }
}