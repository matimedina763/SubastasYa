using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Domain.Entities;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Billeteras.DepositarSaldo;

public class DepositarSaldoCommandHandler
{
    private readonly IBilleteraRepository _billeteraRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DepositarSaldoCommandHandler(IBilleteraRepository billeteraRepository, IUnitOfWork unitOfWork)
    {
        _billeteraRepository = billeteraRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DepositarSaldoCommand command)
    {
        if (command.Monto <= 0)
            throw new DomainException("El monto a depositar debe ser positivo.");

        var billetera = await _billeteraRepository.ObtenerPorUsuarioIdAsync(command.UsuarioId);
        if (billetera is null)
            throw new DomainException("No se encontró una billetera para ese usuario.");

        billetera.SaldoTotal += command.Monto;
        billetera.Version++;

        _billeteraRepository.AgregarMovimientoLedger(new TransaccionLedger
        {
            BilleteraId = billetera.Id,
            Monto = command.Monto,
            Tipo = "DEPOSITO"
        });

        await _unitOfWork.SaveChangesAsync();
    }
}