using System.ComponentModel.DataAnnotations;

namespace MiProyectoApi.Models;

public class Billetera
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public decimal SaldoTotal { get; set; }
    public decimal SaldoRetenido { get; set; }
    public decimal SaldoDisponible => SaldoTotal - SaldoRetenido;

    [Timestamp]
    public byte[] Version { get; set; } = null!;

    public ICollection<TransaccionLedger> Movimientos { get; set; } = new List<TransaccionLedger>();
}