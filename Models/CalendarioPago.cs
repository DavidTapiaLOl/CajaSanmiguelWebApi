using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace CajaSanmiguel;

public class CalendarioPago
{
    [Key]
    public int IdPago { get; set; } // PK id
    public int IdPrestamo { get; set; } // FK llave foranea
        
    public int NumeroCuota { get; set; }
    public DateTime FechaPago { get; set; } // Fecha límite programada
    public decimal MontoCuota { get; set; }
    public bool Pagado { get; set; } = false; // Estado de la cuota

// Propiedades de Navegación
    [ForeignKey("IdPrestamo")]
    public Prestamo Prestamo { get; set; } = null!;
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
