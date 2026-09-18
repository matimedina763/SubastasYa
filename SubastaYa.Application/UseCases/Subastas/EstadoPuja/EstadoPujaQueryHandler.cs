using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Subastas.EstadoPuja;

public class EstadoPujaQueryHandler
{
    private readonly ISubastaRepository _subastaRepository;

    public EstadoPujaQueryHandler(
        ISubastaRepository subastaRepository)
    {
        _subastaRepository = subastaRepository;
    }

    public async Task<EstadoPujaDto> Handle(
        EstadoPujaQuery query)
    {
        var subasta =
            await _subastaRepository.ObtenerSubastaPorIdAsync(
                query.SubastaId);

        if (subasta is null)
        {
            throw new SubastaNoEncontradaException(
                query.SubastaId);
        }

        var pujaLider = subasta.Pujas
            .OrderByDescending(puja => puja.Monto)
            .FirstOrDefault();

        var ofertaActual = subasta.OfertaActual();

        var compradorLidera =
            pujaLider is not null &&
            pujaLider.CompradorId == query.CompradorId;

        var compradorParticipo = subasta.Pujas
            .Any(puja =>
                puja.CompradorId == query.CompradorId);

        return new EstadoPujaDto
        {
            SubastaId = subasta.Id,
            OfertaActual = ofertaActual,
            IncrementoMinimo = subasta.IncrementoMinimo,
            ProximaOferta = ofertaActual + subasta.IncrementoMinimo,
            Liderando = compradorLidera,
            Superado = compradorParticipo && !compradorLidera,
            EstadoSubasta = subasta.Estado,
            FechaFin = subasta.FechaFin
        };
    }
}