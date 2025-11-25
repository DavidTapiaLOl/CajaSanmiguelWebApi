namespace CajaSanmiguel;

public class PagoUpdateDto
{
public DateTime? FechaProgramada { get; set; } // Cuándo DEBE pagar
    public decimal? MontoProgramado { get; set; } // Cuánto DEBE pagar

    // Datos del Pago Real (Se llenan cuando el cliente paga)
    public DateTime? FechaPagoReal { get; set; } // Puede ser nulo si no ha pagado
    public decimal? MontoPagado { get; set; }
    public string? Estado { get; set; } // "Pendiente", "Pagado", "Atrasado"
}
