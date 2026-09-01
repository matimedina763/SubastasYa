namespace MiProyectoApi.Models

public class Puja
{
    public int Id { get; set; }

    public int SubastaId { get; set; }
    public Subasta Subasta { get; set; }

    public int CompradorId { get; set; }
    public usuario Comprador { get; set; }

    public decimal Monto { get; set; }
    public DateTime FechaPuja { get; set; } = DateTime.UtcNow;
}