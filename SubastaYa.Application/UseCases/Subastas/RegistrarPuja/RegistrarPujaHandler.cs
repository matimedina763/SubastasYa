using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Domain.Entities;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Subastas.RegistrarPuja;

public class RegistrarPujaCommandHandler
{
    private readonly ISubastaRepository _subastaRepository;
    private readonly IBilleteraRepository _billeteraRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarPujaCommandHandler(
        ISubastaRepository subastaRepository,
        IBilleteraRepository billeteraRepository,
        IUnitOfWork unitOfWork)
    {
        _subastaRepository = subastaRepository;
        _billeteraRepository = billeteraRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RegistrarPujaCommand command)
    {
        // Paso 1: buscar la subasta
        var subasta = await _subastaRepository.ObtenerSubastaPorIdAsync(command.SubastaId);
        if (subasta is null)
            throw new SubastaNoEncontradaException(command.SubastaId);

        // Paso 2: verificar que esté activa
        if (subasta.Estado != "ACTIVA")
            throw new SubastaNoActivaException();

        // Paso 3: validar el monto contra la regla de negocio de la entidad
        if (!subasta.EsPujaValida(command.Monto))
            throw new PujaInvalidaException(
                $"El monto debe ser al menos {subasta.OfertaActual() + subasta.IncrementoMinimo}.");

        // Paso 4: buscar la billetera del comprador
        var billeteraComprador = await _billeteraRepository.ObtenerPorUsuarioIdAsync(command.CompradorId);
        if (billeteraComprador is null)
            throw new DomainException("El comprador no tiene una billetera asociada.");

        // Paso 5: validar fondos suficientes
        if (billeteraComprador.SaldoDisponible < command.Monto)
            throw new FondosInsuficientesException();

        // Paso 6: liberar la retención del postor anterior (si existe)
        var pujaLiderAnterior = subasta.Pujas
            .OrderByDescending(p => p.Monto)
            .FirstOrDefault();

        if (pujaLiderAnterior is not null)
        {
            var billeteraAnterior = await _billeteraRepository.ObtenerPorUsuarioIdAsync(pujaLiderAnterior.CompradorId);
            if (billeteraAnterior is not null)
            {
                billeteraAnterior.SaldoRetenido -= pujaLiderAnterior.Monto;
                billeteraAnterior.Version++;

                _billeteraRepository.AgregarMovimientoLedger(new TransaccionLedger
                {
                    BilleteraId = billeteraAnterior.Id,
                    Monto = pujaLiderAnterior.Monto,
                    Tipo = "LIBERACION",
                    SubastaId = subasta.Id
                });
            }
        }

        // Paso 7: retener los fondos del nuevo postor
        billeteraComprador.SaldoRetenido += command.Monto;
        billeteraComprador.Version++;

        _billeteraRepository.AgregarMovimientoLedger(new TransaccionLedger
        {
            BilleteraId = billeteraComprador.Id,
            Monto = -command.Monto,
            Tipo = "RETENCION",
            SubastaId = subasta.Id
        });

        // Paso 8: registrar la puja nueva
        var nuevaPuja = new Puja
        {
            SubastaId = subasta.Id,
            CompradorId = command.CompradorId,
            Monto = command.Monto,
            FechaPuja = DateTime.UtcNow
        };
        _subastaRepository.AgregarPuja(nuevaPuja);

        // Paso 9: anti-sniping
        if (subasta.EstaEnVentanaAntiSniping(DateTime.UtcNow))
        {
            subasta.ExtenderCierre();
        }

        // Paso 10: incrementar el Version de la subasta (optimistic locking manual)
        subasta.Version++;

        // Paso 11: guardar todo junto, atómicamente
        await _unitOfWork.SaveChangesAsync();
    }
}