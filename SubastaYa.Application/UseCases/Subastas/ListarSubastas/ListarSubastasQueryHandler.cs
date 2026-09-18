using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces.Persistence;

namespace SubastaYa.Application.UseCases.Subastas.ListarSubastas;

public class ListarSubastasQueryHandler
{
    private readonly ISubastaRepository _subastaRepository;

    public ListarSubastasQueryHandler(ISubastaRepository subastaRepository)
    {
        _subastaRepository = subastaRepository;
    }

    public async Task<List<SubastaDto>> Handle(ListarSubastasQuery query)
    {
        var subastas = await _subastaRepository.ListarAsync(
            query.Estado, query.CategoriaId, query.PrecioMin, query.PrecioMax, query.OrdenarPor);

        return subastas.Select(subasta =>
        {
            var pujaLider = subasta.Pujas.OrderByDescending(puja => puja.Monto).FirstOrDefault();

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
        }).ToList();
    }
}