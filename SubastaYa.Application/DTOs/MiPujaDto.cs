namespace SubastaYa.Application.DTOs;

public class MiPujaDto
{
    public int SubastaId { get; set; }
    public string TituloSubasta { get; set; } = string.Empty;
    public decimal MiUltimaOferta { get; set; }
    public string EstadoSubasta { get; set; } = string.Empty;
    public bool SoyElLider { get; set; }
}