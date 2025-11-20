using System.ComponentModel.DataAnnotations;
namespace CajaSanmiguel;

public class Usuario
{
    [Key]
    public int IdUsuario { get; set; } // pk ID 
    public string Nombre { get; set; }
    public string Correo { get; set; }
    public string Password { get; set; }
}
