namespace SubastaYa.Application.DTOs;

public class PujaDto
{
    public int Id { get; set; }
    public decimal Monto { get; set; }
    public string Postor { get; set; } = string.Empty;
    public DateTime FechaPuja { get; set; }
}