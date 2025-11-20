using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace CajaSanmiguel;

public class Prestamo
{
    [Key]
    public int IdPrestamo { get; set; } // PK id
    public int IdCliente { get; set; } // FK llave foranea
        
    public decimal Monto { get; set; }
    public decimal Interes { get; set; }
    public decimal TotalAPagar { get; set; }
    public int NumeroCuotas { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Estado { get; set; } // activo, pendiente, vencido

    // Propiedades de Navegación
    [ForeignKey("IdCliente")]
    public Cliente Cliente { get; set; } = null!;   
    public ICollection<CalendarioPago> CalendarioPagos { get; set; } = new List<CalendarioPago>();
}
