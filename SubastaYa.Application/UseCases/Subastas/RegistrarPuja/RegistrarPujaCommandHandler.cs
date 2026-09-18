using System.Text.Json;
using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Domain.Entities;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Subastas.RegistrarPuja;

public class RegistrarPujaCommandHandler
{
    private readonly ISubastaRepository _subastaRepository;
    private readonly IBilleteraRepository _billeteraRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditoriaLogRepository _auditoriaLogRepository;

    public RegistrarPujaCommandHandler(
        ISubastaRepository subastaRepository,
        IBilleteraRepository billeteraRepository,
        IUnitOfWork unitOfWork,
        IAuditoriaLogRepository auditoriaLogRepository)
    {
        _subastaRepository = subastaRepository;
        _billeteraRepository = billeteraRepository;
        _unitOfWork = unitOfWork;
        _auditoriaLogRepository = auditoriaLogRepository;
    }

    public async Task<RegistrarPujaResultado> Handle(RegistrarPujaCommand command)
    {
        // - VALIDACION 1: ¿Existe la subasta? -
        var subasta =
            await _subastaRepository.ObtenerSubastaPorIdAsync(
                command.SubastaId);

        if (subasta is null)
        {
            await RegistrarPujaRechazadaAsync(
                command,
                "SUBASTA_NO_ENCONTRADA");

            throw new SubastaNoEncontradaException(command.SubastaId);
        }

        // -- VALIDACION 2: ¿ESTA ACTIVA? ---------
        if (subasta.Estado != "ACTIVA")
        {
            await RegistrarPujaRechazadaAsync(
                command,
                "SUBASTA_NO_ACTIVA");

            throw new SubastaNoActivaException();
        }

        // -- El vendedor no puja sobre su propia subasta ------
        if (subasta.VendedorId == command.CompradorId)
        {
            await RegistrarPujaRechazadaAsync(
                command,
                "VENDEDOR_INTENTA_PUJAR");

            throw new PujaInvalidaException(
                "El vendedor no puede pujar en su propia subasta.");
        }

        // --- VALIDACION 3: ¿El monto alcanza? ---
        if (!subasta.EsPujaValida(command.Monto))
        {
            await RegistrarPujaRechazadaAsync(
                command,
                "MONTO_INVALIDO");

            throw new PujaInvalidaException(
                $"El monto debe ser al menos " +
                $"{subasta.OfertaActual() + subasta.IncrementoMinimo}.");
        }

        // --- VALIDACION 4: ¿El comprador tiene billetera? ---
        var billeteraComprador =
            await _billeteraRepository.ObtenerPorUsuarioIdAsync(
                command.CompradorId);

        if (billeteraComprador is null)
        {
            await RegistrarPujaRechazadaAsync(
                command,
                "BILLETERA_NO_ENCONTRADA");

            throw new DomainException(
                "El comprador no tiene una billetera asociada.");
        }

        // --- VALIDACION 5: ¿Tiene fondos suficientes? --
        if (billeteraComprador.SaldoDisponible < command.Monto)
        {
            await RegistrarPujaRechazadaAsync(
                command,
                "FONDOS_INSUFICIENTES");

            throw new FondosInsuficientesException();
        }

        // ACCION 1 - Liberar la retención del postor anterior
        var resultado =
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var pujaLiderAnterior = subasta.Pujas
                    .OrderByDescending(puja => puja.Monto)
                    .FirstOrDefault();

                if (pujaLiderAnterior is not null)
                {
                    var billeteraAnterior =
                        await _billeteraRepository
                            .ObtenerPorUsuarioIdAsync(
                                pujaLiderAnterior.CompradorId);

                    if (billeteraAnterior is not null)
                    {
                        billeteraAnterior.SaldoRetenido -=
                            pujaLiderAnterior.Monto;

                        billeteraAnterior.Version++;

                        _billeteraRepository.AgregarMovimientoLedger(
                            new TransaccionLedger
                            {
                                BilleteraId = billeteraAnterior.Id,
                                Monto = pujaLiderAnterior.Monto,
                                Tipo = "LIBERACION",
                                SubastaId = subasta.Id
                            });
                    }
                }

                // ACCION 2: Retener los fondos del nuevo postor.
                billeteraComprador.SaldoRetenido += command.Monto;
                billeteraComprador.Version++;

                _billeteraRepository.AgregarMovimientoLedger(
                    new TransaccionLedger
                    {
                        BilleteraId = billeteraComprador.Id,
                        Monto = -command.Monto,
                        Tipo = "RETENCION",
                        SubastaId = subasta.Id
                    });

                // ACCION 3: Registrar la nueva puja.
                var nuevaPuja = new Puja
                {
                    SubastaId = subasta.Id,
                    CompradorId = command.CompradorId,
                    Monto = command.Monto,
                    FechaPuja = DateTime.UtcNow
                };

                _subastaRepository.AgregarPuja(nuevaPuja);

                // ACCION 4: Chequeo Anti-Sniping
                bool seExtendio = false;

                if (subasta.EstaEnVentanaAntiSniping(DateTime.UtcNow))
                {
                    var fechaAnterior = subasta.FechaFin;

                    subasta.ExtenderCierre();
                    seExtendio = true;

                    _auditoriaLogRepository.Agregar(
                        new AuditoriaLog
                        {
                            Entidad = "SUBASTA",
                            EntidadId = subasta.Id,
                            UsuarioId = command.CompradorId,
                            Accion = "EXTENSION_ANTI_SNIPING",
                            Fecha = DateTime.UtcNow,
                            DetalleJson = JsonSerializer.Serialize(
                                new
                                {
                                    fechaAnterior,
                                    fechaNueva = subasta.FechaFin,
                                    segundosAgregados = 120
                                })
                        });
                }

                subasta.Version++;    // ACCION 5: Incrementar la Version de la subasta

                return new RegistrarPujaResultado
                {
                    PujaId = nuevaPuja.Id,
                    SubastaExtendida = seExtendio,
                    NuevaFechaFin = subasta.FechaFin
                };
            });

        return resultado;
    }

    private async Task RegistrarPujaRechazadaAsync(
        RegistrarPujaCommand command,
        string motivo)
    {
        _auditoriaLogRepository.Agregar(
            new AuditoriaLog
            {
                Entidad = "PUJA",
                EntidadId = command.SubastaId,
                UsuarioId = command.CompradorId,
                Accion = "PUJA_RECHAZADA",
                Fecha = DateTime.UtcNow,
                DetalleJson = JsonSerializer.Serialize(
                    new
                    {
                        motivo,
                        monto = command.Monto
                    })
            });

        await _unitOfWork.SaveChangesAsync();
    }
}