namespace SubastaYa.Application.UseCases.Billeteras.DepositarSaldo;

public class DepositarSaldoCommand
{
    public int UsuarioId { get; set; }
    public decimal Monto { get; set; }
}