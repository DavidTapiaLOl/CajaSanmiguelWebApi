using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CajaSanmiguel;

public class Pago
{
    [Key]
    public int IdPago { get; set; } // PK (Antes IdRegistroPago)
    public int IdPrestamo { get; set; } 
    public int NumeroCuota { get; set; }
    public DateTime FechaProgramada { get; set; }
    public decimal MontoProgramado { get; set; } 

    // Datos del Pago Real (Se llenan cuando el cliente paga)
    public DateTime? FechaPagoReal { get; set; }
    public decimal? MontoPagado { get; set; }
    public string Estado { get; set; } // "Pendiente", "Pagado", "Atrasado"

    //RELACIÓN
    [ForeignKey("IdPrestamo")]
    [JsonIgnore]
    public Prestamo? Prestamo { get; set; }
}