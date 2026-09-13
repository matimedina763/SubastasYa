using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces.Persistence;

public interface ISubastaRepository
{
    Task<Subasta?> ObtenerSubastaPorIdAsync(int id);
    void AgregarPuja(Puja puja);
    Task<List<Subasta>> ObtenerActivasVencidasAsync(DateTime ahora);

}
