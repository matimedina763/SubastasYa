namespace SubastaYa.Application.DTOs;

public class EstadoPujaDto
{
    public int SubastaId { get; set; }
    public decimal OfertaActual { get; set; }
    public decimal IncrementoMinimo { get; set; }
    public decimal ProximaOferta { get; set; }
    public bool Liderando { get; set; }
    public bool Superado { get; set; }
    public string EstadoSubasta { get; set; } = string.Empty;
    public DateTime FechaFin { get; set; }
}