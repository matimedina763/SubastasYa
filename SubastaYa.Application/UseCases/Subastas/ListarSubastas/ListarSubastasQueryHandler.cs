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

        return subastas.Select(s =>
        {
            var pujaLider = s.Pujas.OrderByDescending(p => p.Monto).FirstOrDefault();

            return new SubastaDto
            {
                Id = s.Id,
                Titulo = s.Titulo,
                Descripcion = s.Descripcion,
                PrecioInicial = s.PrecioBase,
                OfertaActual = s.OfertaActual(),
                FechaInicio = s.FechaInicio,
                FechaFin = s.FechaFin,
                Activa = s.Estado == "ACTIVA",
                Estado = s.Estado,
                LiderNombre = pujaLider?.Comprador?.Nombre  // null si nadie pujó todavía
            };
        }).ToList();
    }
}