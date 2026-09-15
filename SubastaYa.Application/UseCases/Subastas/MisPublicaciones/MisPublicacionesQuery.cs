namespace SubastaYa.Application.UseCases.Subastas.MisPublicaciones;

public class MisPublicacionesQuery
{
    public int VendedorId { get; set; }
    public MisPublicacionesQuery(int vendedorId) => VendedorId = vendedorId;
}