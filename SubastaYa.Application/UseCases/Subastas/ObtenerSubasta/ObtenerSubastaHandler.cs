using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Domain.Entities;
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
        Subasta? subasta =
            await _subastaRepository.ObtenerSubastaPorIdAsync(request.Id);

        if (subasta is null)
        {
            throw new SubastaNoEncontradaException(request.Id);
        }

        Puja? pujaLider = subasta.Pujas
            .OrderByDescending(puja => puja.Monto)
            .FirstOrDefault();

        return new SubastaDto
        {
            Id = subasta.Id,
            Titulo = subasta.Titulo,
            Descripcion = subasta.Descripcion,
            UrlImagen = subasta.UrlImagen,
            Categoria = subasta.Categoria?.Nombre,
            PrecioInicial = subasta.PrecioBase,
            OfertaActual = subasta.OfertaActual(),
            CantidadOfertas = subasta.Pujas.Count,
            FechaInicio = subasta.FechaInicio,
            FechaFin = subasta.FechaFin,
            Activa = subasta.Estado == "ACTIVA",
            Estado = subasta.Estado,
            LiderNombre = pujaLider?.Comprador?.Nombre
        };
    }
}