using MediatR;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.UseCases.Subastas.ObtenerSubasta;
public class ObtenerSubastaHandler : IRequestHandler<ObtenerSubastaQuery, SubastaDto>
{
    private readonly ISubastaRepository _subastaRepository;

    public ObtenerSubastaHandler(ISubastaRepository subastaRepository)
    {
        _subastaRepository = subastaRepository;
    }

    public async Task<SubastaDto> Handle(ObtenerSubastaQuery request, CancellationToken cancellationToken)
    {
        var subasta = await _subastaRepository.ObtenerSubastaPorIdAsync(request.Id);

        if (subasta == null)
        {
            throw new Exception("Subasta no encontrada");
        }

        return new SubastaDto
        {
            Id = subasta.Id,
            Titulo = subasta.Titulo,
            Descripcion = subasta.Descripcion,
            PrecioInicial = subasta.precio_base,
            FechaInicio = subasta.fecha_inicio,
            FechaFin = subasta.fecha_fin,
            Activa = subasta.estado == "ACTIVA"
        };
    }
}


                                              
