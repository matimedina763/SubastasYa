using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces.Persistence;

namespace SubastaYa.Application.UseCases.Subastas.MisPublicaciones;

public class MisPublicacionesQueryHandler
{
    private readonly ISubastaRepository _subastaRepository;

    public MisPublicacionesQueryHandler(ISubastaRepository subastaRepository)
    {
        _subastaRepository = subastaRepository;
    }

    public async Task<List<MiPublicacionDto>> Handle(MisPublicacionesQuery query)
    {
        var subastas = await _subastaRepository.ObtenerPorVendedorIdAsync(query.VendedorId);

        return subastas.Select(s =>
        {
            var pujaGanadora = s.Estado == "FINALIZADA"
                ? s.Pujas.OrderByDescending(p => p.Monto).FirstOrDefault()
                : null;

            return new MiPublicacionDto
            {
                SubastaId = s.Id,
                Titulo = s.Titulo,
                Estado = s.Estado,
                PrecioBase = s.PrecioBase,
                MontoRecaudado = pujaGanadora?.Monto ?? 0,
                CantidadPujas = s.Pujas.Count
            };
        }).ToList();
    }
}