using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces.Persistence;

public interface IBilleteraRepository
{
    Task<Billetera?> ObtenerPorUsuarioIdAsync(int usuarioId);
    void AgregarMovimientoLedger(TransaccionLedger movimiento);
    Task<List<TransaccionLedger>> ObtenerMovimientosPorUsuarioIdAsync(int usuarioId);
}