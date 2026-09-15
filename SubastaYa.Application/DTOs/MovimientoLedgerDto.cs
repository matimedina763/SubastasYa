namespace SubastaYa.Application.DTOs;

public class MovimientoLedgerDto
{
    public string Tipo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
    public int? SubastaId { get; set; }
}