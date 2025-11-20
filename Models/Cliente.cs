using System.ComponentModel.DataAnnotations;
namespace CajaSanmiguel;

public class Cliente
{
    [Key]
    public int IdCliente { get; set; } // pk ID 
    public string Nombre { get; set; }
    public string Apellidos { get; set; }
    public string Telefono { get; set; }
    public string Direccion { get; set; }

    // Propiedad de Navegación: Un cliente puede tener muchos préstamos
    public ICollection<Prestamo> Prestamos { get; set; }
}
