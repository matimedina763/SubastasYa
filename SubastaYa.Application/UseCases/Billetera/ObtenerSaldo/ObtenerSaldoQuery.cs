namespace SubastaYa.Application.UseCases.Billetera.ObtenerSaldo;

public class ObtenerSaldoQuery
{
    public int UsuarioId { get; set; }
    public ObtenerSaldoQuery(int usuarioId) => UsuarioId = usuarioId;
}