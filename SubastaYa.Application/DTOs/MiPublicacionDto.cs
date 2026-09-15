namespace SubastaYa.Application.DTOs;

public class MiPublicacionDto
{
    public int SubastaId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public decimal PrecioBase { get; set; }
    public decimal MontoRecaudado { get; set; } // 0 si no se vendió, el monto de la puja ganadora si se vendió
    public int CantidadPujas { get; set; }
}