namespace SubastaYa.Application.UseCases.Billeteras.ObtenerMovimientos;

public class ObtenerMovimientosQuery
{
    public int UsuarioId { get; set; }
    public ObtenerMovimientosQuery(int usuarioId) => UsuarioId = usuarioId;
}