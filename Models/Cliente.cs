using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace CajaSanmiguel;
public class Cliente
{
    [Key]
    public int IdCliente { get; set; } // pk ID 
    public string Nombre { get; set; }
    public string Apellidos { get; set; }
    public string Telefono { get; set; }
    public string Direccion { get; set; }
    public List<Prestamo>? Prestamos {get;set;} = new List<Prestamo>();
}
