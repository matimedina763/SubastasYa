namespace SubastaYa.Application.UseCases.Subastas.MisPujas;

public class MisPujasQuery
{
    public int UsuarioId { get; set; }
    public MisPujasQuery(int usuarioId) => UsuarioId = usuarioId;
}