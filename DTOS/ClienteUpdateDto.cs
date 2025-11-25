namespace CajaSanmiguel;

public class ClienteUpdateDto
{
    // Usamos 'string?' (nullable) para saber si enviaron el dato o no.
    public string? Nombre { get; set; }
    public string? Apellidos { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
}
