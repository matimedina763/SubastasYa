using System.Text.Json;
using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Domain.Entities;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Billeteras.DepositarSaldo;

public class DepositarSaldoCommandHandler
{
    private readonly IBilleteraRepository _billeteraRepository;
    private readonly IAuditoriaLogRepository _auditoriaLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DepositarSaldoCommandHandler(IBilleteraRepository billeteraRepository, IAuditoriaLogRepository auditoriaLogRepository, IUnitOfWork unitOfWork)
    {
        _billeteraRepository = billeteraRepository;
        _auditoriaLogRepository = auditoriaLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DepositarSaldoCommand command)
    {
        if (command.Monto <= 0)
        {
            throw new DomainException(
                "El monto a depositar debe ser positivo.");
        }        

        var billetera = await _billeteraRepository.ObtenerPorUsuarioIdAsync(command.UsuarioId);
        if (billetera is null)
        {
            throw new DomainException("No se encontró una billetera para ese usuario.");
        }

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            billetera.SaldoTotal += command.Monto;
            billetera.Version++;        // Esta operación guarda el incremento de Version

            _billeteraRepository.AgregarMovimientoLedger(     // Se deposita todo en el Ledger
                new TransaccionLedger
                {
                    BilleteraId = billetera.Id,
                    Monto = command.Monto,
                    Tipo = "DEPOSITO"
                });
            _auditoriaLogRepository.Agregar(                  // Se acredita manualmente en Auditoría
                new AuditoriaLog
                {
                    Entidad = "BILLETERA",
                    EntidadId = billetera.Id,
                    UsuarioId = command.UsuarioId,
                    Accion = "ACREDITACION_MANUAL",
                    Fecha = DateTime.UtcNow,
                    DetalleJson = JsonSerializer.Serialize(
                        new
                        {
                            monto = command.Monto,
                            tipo = "DEPOSITO"
                        })    
                });    
            return true;
        });
    }
}    