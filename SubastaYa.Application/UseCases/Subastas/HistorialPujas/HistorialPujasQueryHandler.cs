using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Subastas.HistorialPujas;

public class HistorialPujasQueryHandler
{
    private readonly ISubastaRepository _subastaRepository;

    public HistorialPujasQueryHandler(
        ISubastaRepository subastaRepository)
    {
        _subastaRepository = subastaRepository;
    }

    public async Task<List<PujaDto>> Handle(
        HistorialPujasQuery query)
    {
        var subasta =
            await _subastaRepository.ObtenerSubastaPorIdAsync(
                query.SubastaId);

        if (subasta is null)
        {
            throw new SubastaNoEncontradaException(
                query.SubastaId);
        }

        return subasta.Pujas
            .OrderBy(puja => puja.FechaPuja)
            .Select(puja => new PujaDto
            {
                Id = puja.Id,
                Monto = puja.Monto,
                Postor = $"Postor {puja.CompradorId}",
                FechaPuja = puja.FechaPuja
            })
            .ToList();
    }
}