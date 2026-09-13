using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.UseCases.Subastas.CerrarSubastasVencidas;

public class CerrarSubastasVencidasCommandHandler
{
    private readonly ISubastaRepository _subastaRepository;
    private readonly IBilleteraRepository _billeteraRepository;
    private readonly IAuditoriaLogRepository _auditoriaLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CerrarSubastasVencidasCommandHandler(
        ISubastaRepository subastaRepository,
        IBilleteraRepository billeteraRepository,
        IAuditoriaLogRepository auditoriaLogRepository,
        IUnitOfWork unitOfWork)
    {
        _subastaRepository = subastaRepository;
        _billeteraRepository = billeteraRepository;
        _auditoriaLogRepository = auditoriaLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CerrarSubastasVencidasCommand command)
    {
        // Trae todas las subastas ACTIVA cuyo FechaFin ya pasó
        var subastasVencidas = await _subastaRepository.ObtenerActivasVencidasAsync(command.Ahora);

        foreach (var subasta in subastasVencidas)
        {
            var pujaGanadora = subasta.Pujas.OrderByDescending(p => p.Monto).FirstOrDefault();

            if (pujaGanadora is null)
            {
                // ── CASO: nadie ofertó -> DESIERTA ──────────────────────
                subasta.Estado = "DESIERTA";
                subasta.Version++;

                _auditoriaLogRepository.Agregar(new AuditoriaLog
                {
                    Entidad = "SUBASTA",
                    EntidadId = subasta.Id,
                    Accion = "PASE_A_DESIERTA",
                    Fecha = command.Ahora,
                    DetalleJson = $"{{\"motivo\":\"sin pujas registradas\"}}"
                });
            }
            else
            {
                // ── CASO: hay ganador -> liquidar y FINALIZAR ───────────
                var billeteraComprador = await _billeteraRepository.ObtenerPorUsuarioIdAsync(pujaGanadora.CompradorId);
                var billeteraVendedor = await _billeteraRepository.ObtenerPorUsuarioIdAsync(subasta.VendedorId);

                if (billeteraComprador is not null && billeteraVendedor is not null)
                {
                    // Debitar al comprador: se descuenta de Total y de Retenido a la vez
                    billeteraComprador.SaldoTotal -= pujaGanadora.Monto;
                    billeteraComprador.SaldoRetenido -= pujaGanadora.Monto;
                    billeteraComprador.Version++;

                    // Acreditar al vendedor
                    billeteraVendedor.SaldoTotal += pujaGanadora.Monto;
                    billeteraVendedor.Version++;

                    _billeteraRepository.AgregarMovimientoLedger(new TransaccionLedger
                    {
                        BilleteraId = billeteraComprador.Id,
                        Monto = -pujaGanadora.Monto,
                        Tipo = "PAGO",
                        SubastaId = subasta.Id
                    });

                    _billeteraRepository.AgregarMovimientoLedger(new TransaccionLedger
                    {
                        BilleteraId = billeteraVendedor.Id,
                        Monto = pujaGanadora.Monto,
                        Tipo = "COBRO",
                        SubastaId = subasta.Id
                    });
                }

                subasta.Estado = "FINALIZADA";
                subasta.Version++;

                _auditoriaLogRepository.Agregar(new AuditoriaLog
                {
                    Entidad = "SUBASTA",
                    EntidadId = subasta.Id,
                    Accion = "CIERRE_WORKER",
                    Fecha = command.Ahora,
                    DetalleJson = $"{{\"ganadorId\":{pujaGanadora.CompradorId},\"monto\":{pujaGanadora.Monto}}}"
                });
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return subastasVencidas.Count; // cuántas se procesaron, útil para logging
    }
}