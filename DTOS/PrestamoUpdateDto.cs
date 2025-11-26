namespace CajaSanmiguel;

public class PrestamoUpdateDto
{
    public int? IdCliente { get; set; } // FK hacia Cliente
    public decimal? Monto { get; set; }
    public decimal? Interes { get; set; }
    public int? NumeroCuotas { get; set; }
    public string? Lapzo { get; set; } // Semanal, Quincenal, Mensual
    public decimal? MontoMulta { get; set; }
    public DateTime? FechaInicio { get; set; }
    public string? Estado { get; set; } // Activo, Terminado
}
