using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace CajaSanmiguel;

public class Pago
{
   /*  [Key]
    public int IdRegistroPago { get; set; } // PK
    public int IdPrestamo { get; set; } // FK (Redundante, pero útil para consultas)
    public int IdPago { get; set; } // FK (Referencia a la cuota específica)

    public DateTime FechaPagoReal { get; set; } //Fecha del pago
    public decimal MontoMultaAplicado { get; set; } //Multas: Monto de la multa
    public decimal MontoPagado { get; set; } // Monto total (Cuota + Multa)

    public CalendarioPago Cuota { get; set; }

    [ForeignKey("IdPrestamo")]
    public Prestamo Prestamo { get; set; } = null!;   */

    [Key]
        public int IdRegistroPago { get; set; } // PK

        public int IdPago { get; set; }     // FK a CalendarioPago

        public DateTime FechaPagoReal { get; set; }
        public decimal MontoMultaAplicado { get; set; }
        public decimal MontoPagado { get; set; }

        // --- Propiedades de Navegación ---

        [ForeignKey("IdPago")]
        public CalendarioPago Cuota { get; set; } = null!;
}
