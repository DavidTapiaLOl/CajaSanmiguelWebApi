namespace CajaSanmiguel;

public class PagoUpdateDto
{
public DateTime? FechaProgramada { get; set; }
    public decimal? MontoProgramado { get; set; }

    // Datos del Pago Real (Se llenan cuando el cliente paga)
    public DateTime? FechaPagoReal { get; set; }
    public decimal? MontoPagado { get; set; }
    public string? Estado { get; set; } // "Pendiente", "Pagado", "Atrasado"
}
