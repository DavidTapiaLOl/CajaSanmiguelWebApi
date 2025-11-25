using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; // Importante para evitar ciclos

namespace CajaSanmiguel;

public class Prestamo
{
    [Key]
    public int IdPrestamo { get; set; } 

    public int IdCliente { get; set; } // FK hacia Cliente
    public decimal Monto { get; set; }
    public decimal Interes { get; set; }
    public int NumeroCuotas { get; set; }
    public string Lapzo { get; set; } // Semanal, Quincenal, Mensual
    public decimal MontoMulta { get; set; } 
    public decimal TotalAPagar { get; set; }
    public DateTime FechaInicio { get; set; }
    public string Estado { get; set; } // Activo, Terminado

    // --- RELACIONES ---

    [ForeignKey("IdCliente")]
    [JsonIgnore]
    public Cliente? Cliente { get; set; }  

    // Relación 1 a Muchos: Un Préstamo tiene una lista de Pagos.
    // EF Core buscará "IdPrestamo" en la tabla Pago automáticamente.
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}