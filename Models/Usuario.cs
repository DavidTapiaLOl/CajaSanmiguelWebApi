using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; // NECESARIO PARA [JsonIgnore]

namespace CajaSanmiguel;

public class Usuario
{
    [Key]
    public int IdUsuario { get; set; } 
    
    public string Nombre { get; set; }
    
    public string Correo { get; set; }
    
    [JsonIgnore]//para que no lo muestre en la peticion
    public string Password { get; set; } 
    
    public string Rol { get; set; } // Admin, Cajero
}

public class UsuarioRegistroDto
{
    public string Nombre { get; set; }
    public string Correo { get; set; }
    public string Password { get; set; }
    public string Rol { get; set; }
}