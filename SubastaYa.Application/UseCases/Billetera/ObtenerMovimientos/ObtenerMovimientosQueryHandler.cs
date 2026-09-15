using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces.Persistence;

namespace SubastaYa.Application.UseCases.Billeteras.ObtenerMovimientos;

public class ObtenerMovimientosQueryHandler
{
    private readonly IBilleteraRepository _billeteraRepository;

    public ObtenerMovimientosQueryHandler(IBilleteraRepository billeteraRepository)
    {
        _billeteraRepository = billeteraRepository;
    }

    public async Task<List<MovimientoLedgerDto>> Handle(ObtenerMovimientosQuery query)
    {
        var movimientos = await _billeteraRepository.ObtenerMovimientosPorUsuarioIdAsync(query.UsuarioId);

        return movimientos
            .OrderByDescending(m => m.Fecha)
            .Select(m => new MovimientoLedgerDto
            {
                Tipo = m.Tipo,
                Monto = m.Monto,
                Fecha = m.Fecha,
                SubastaId = m.SubastaId
            })
            .ToList();
    }
}