using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Subastas.ObtenerSubasta;

public class ObtenerSubastaHandler
{
    private readonly ISubastaRepository _subastaRepository;

    public ObtenerSubastaHandler(ISubastaRepository subastaRepository)
    {
        _subastaRepository = subastaRepository;
    }

    public async Task<SubastaDto> Handle(ObtenerSubastaQuery request)
    {
        var subasta = await _subastaRepository.ObtenerSubastaPorIdAsync(request.Id);

        if (subasta == null)
        {
            throw new SubastaNoEncontradaException(request.Id);
        }

        return new SubastaDto
        {
            Id = subasta.Id,
            Titulo = subasta.Titulo,
            Descripcion = subasta.Descripcion,
            PrecioInicial = subasta.PrecioBase,
            FechaInicio = subasta.FechaInicio,
            FechaFin = subasta.FechaFin,
            Activa = subasta.Estado == "ACTIVA"
        };
    }
}