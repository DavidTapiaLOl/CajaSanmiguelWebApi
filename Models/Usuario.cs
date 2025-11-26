using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; // NECESARIO PARA [JsonIgnore]

namespace CajaSanmiguel;

public class Usuario
{
    [Key]
    public int IdUsuario { get; set; } 
    
    public string Nombre { get; set; }
    
    public string Correo { get; set; } // Usaremos "Correo" en el Controller
    
    [JsonIgnore] // 🔒 IMPORTANTE: Esto evita que la contraseña se envíe al frontend
    public string Password { get; set; } 
    
    public string Rol { get; set; } // Admin, Cajero, etc.
}

public class UsuarioRegistroDto
{
    public string Nombre { get; set; }
    public string Correo { get; set; }
    public string Password { get; set; } // 👈 AQUÍ NO PONEMOS [JsonIgnore]
    public string Rol { get; set; }
}