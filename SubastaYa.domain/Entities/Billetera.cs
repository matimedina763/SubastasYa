using System.ComponentModel.DataAnnotations;

namespace SubastaYa.Domain.Entities;

public class Billetera
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public decimal SaldoTotal { get; set; }
    public decimal SaldoRetenido { get; set; }
    public decimal SaldoDisponible => SaldoTotal - SaldoRetenido;

    [ConcurrencyCheck]
    public int Version { get; set; }

    public ICollection<TransaccionLedger> Movimientos { get; set; } = new List<TransaccionLedger>();
}