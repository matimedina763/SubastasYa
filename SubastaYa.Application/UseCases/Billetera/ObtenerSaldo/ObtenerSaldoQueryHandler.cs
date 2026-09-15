using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Application.UseCases.Billetera.ObtenerSaldo;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Billeteras.ObtenerSaldo;

public class ObtenerSaldoQueryHandler
{
    private readonly IBilleteraRepository _billeteraRepository;

    public ObtenerSaldoQueryHandler(IBilleteraRepository billeteraRepository)
    {
        _billeteraRepository = billeteraRepository;
    }

    public async Task<BilleteraDto> Handle(ObtenerSaldoQuery query)
    {
        var billetera = await _billeteraRepository.ObtenerPorUsuarioIdAsync(query.UsuarioId);
        if (billetera is null)
            throw new DomainException("No se encontró una billetera para ese usuario.");

        return new BilleteraDto
        {
            Id = billetera.Id,
            UsuarioId = billetera.UsuarioId,
            SaldoTotal = billetera.SaldoTotal,
            SaldoRetenido = billetera.SaldoRetenido,
            SaldoDisponible = billetera.SaldoDisponible
        };
    }
}