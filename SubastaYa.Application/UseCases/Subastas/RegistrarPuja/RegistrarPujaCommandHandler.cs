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

    public async Task<int> Handle(RegistrarPujaCommand command)
    {
        // ── VALIDACIÓN 1: ¿existe la subasta? ──────────────────────────
        // Si no existe, no tiene sentido seguir evaluando nada más.
        // Se corta acá con una excepción específica -> el middleware la traduce a 404.
        var subasta = await _subastaRepository.ObtenerSubastaPorIdAsync(command.SubastaId);
        if (subasta is null)
            throw new SubastaNoEncontradaException(command.SubastaId);

        // ── VALIDACIÓN 2: ¿está ACTIVA? ─────────────────────────────────
        // No se puede pujar en una subasta PROGRAMADA (todavía no arrancó)
        // ni en una FINALIZADA/DESIERTA (ya cerró). -> 400 vía DomainException.
        if (subasta.Estado != "ACTIVA")
            throw new SubastaNoActivaException();

        // ── VALIDACIÓN 3: ¿el monto alcanza? ────────────────────────────
        // Usa la regla de negocio que vive en la propia entidad Subasta
        // (oferta actual + incremento mínimo). No se recalcula acá,
        // se delega a la entidad -> Rich Domain Model.
        if (!subasta.EsPujaValida(command.Monto))
            throw new PujaInvalidaException(
                $"El monto debe ser al menos {subasta.OfertaActual() + subasta.IncrementoMinimo}.");

        // ── VALIDACIÓN 4: ¿el comprador tiene billetera? ────────────────
        // Caso borde: no debería pasar en la práctica (todo usuario tiene
        // billetera por diseño), pero se chequea para no explotar con
        // un NullReferenceException más abajo.
        var billeteraComprador = await _billeteraRepository.ObtenerPorUsuarioIdAsync(command.CompradorId);
        if (billeteraComprador is null)
            throw new DomainException("El comprador no tiene una billetera asociada.");

        // ── VALIDACIÓN 5: ¿tiene fondos suficientes? ────────────────────
        // Se compara contra SaldoDisponible (= SaldoTotal - SaldoRetenido),
        // NUNCA contra SaldoTotal directo -> es la esencia del escrow:
        // la plata ya comprometida en otra subasta no cuenta como disponible.
        if (billeteraComprador.SaldoDisponible < command.Monto)
            throw new FondosInsuficientesException();

        // ── ACCIÓN 1: liberar la retención del postor anterior ──────────
        // Si había alguien liderando antes, hay que devolverle su saldo
        // retenido ANTES de congelar el del nuevo postor. Se busca la
        // puja de mayor monto ya cargada en la subasta (el líder actual).
        var pujaLiderAnterior = subasta.Pujas.OrderByDescending(p => p.Monto).FirstOrDefault();
        if (pujaLiderAnterior is not null)
        {
            var billeteraAnterior = await _billeteraRepository.ObtenerPorUsuarioIdAsync(pujaLiderAnterior.CompradorId);
            if (billeteraAnterior is not null)
            {
                billeteraAnterior.SaldoRetenido -= pujaLiderAnterior.Monto; // libera el monto retenido
                billeteraAnterior.Version++;                                // optimistic locking manual (SQLite no lo hace solo)

                _billeteraRepository.AgregarMovimientoLedger(new TransaccionLedger
                {
                    BilleteraId = billeteraAnterior.Id,
                    Monto = pujaLiderAnterior.Monto, // positivo: vuelve a estar disponible
                    Tipo = "LIBERACION",
                    SubastaId = subasta.Id
                });
            }
        }

        // ── ACCIÓN 2: retener los fondos del nuevo postor ───────────────
        // Se hace DESPUÉS de liberar al anterior, nunca antes -> evita que
        // en algún punto intermedio haya dos personas con plata retenida
        // por la misma subasta al mismo tiempo.
        billeteraComprador.SaldoRetenido += command.Monto;
        billeteraComprador.Version++;

        _billeteraRepository.AgregarMovimientoLedger(new TransaccionLedger
        {
            BilleteraId = billeteraComprador.Id,
            Monto = -command.Monto, // negativo: sale del disponible
            Tipo = "RETENCION",
            SubastaId = subasta.Id
        });

        // ── ACCIÓN 3: registrar la puja nueva ────────────────────────────
        var nuevaPuja = new Puja
        {
            SubastaId = subasta.Id,
            CompradorId = command.CompradorId,
            Monto = command.Monto,
            FechaPuja = DateTime.UtcNow
        };
        _subastaRepository.AgregarPuja(nuevaPuja);

        // ── ACCIÓN 4: chequeo anti-sniping ──────────────────────────────
        // Si la puja entra dentro de los últimos 60 segundos antes del
        // cierre, se extienden 2 minutos más (regla del enunciado 2.2).
        if (subasta.EstaEnVentanaAntiSniping(DateTime.UtcNow))
        {
            subasta.ExtenderCierre();
        }

        // ── ACCIÓN 5: incrementar el Version de la subasta ──────────────
        // Igual que con las billeteras: en SQLite el optimistic locking
        // no es automático, hay que subir el contador a mano.
        subasta.Version++;

        // ── PERSISTENCIA: guardar todo junto, atómicamente ──────────────
        // Un solo SaveChangesAsync() para TODOS los cambios acumulados
        // arriba (liberación, retención, puja nueva, extensión de cierre,
        // ambos Version). O se guarda todo, o no se guarda nada -> ACID.
        await _unitOfWork.SaveChangesAsync();

        // El Id de la puja recién se completa DESPUÉS del SaveChanges
        // (la base lo autogenera al insertar). Se devuelve para que el
        // Controller arme la respuesta 201 Created con la ubicación del recurso.
        return nuevaPuja.Id;
    }
}