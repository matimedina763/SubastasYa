using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces.Persistence;

namespace SubastaYa.Application.UseCases.Subastas.MisPujas;

public class MisPujasQueryHandler
{
    private readonly ISubastaRepository _subastaRepository;

    public MisPujasQueryHandler(ISubastaRepository subastaRepository)
    {
        _subastaRepository = subastaRepository;
    }

    public async Task<List<MiPujaDto>> Handle(MisPujasQuery query)
    {
        var subastas = await _subastaRepository.ObtenerSubastasConPujaDeUsuarioAsync(query.UsuarioId);

        return subastas.Select(s =>
        {
            // Mi última oferta en ESTA subasta puntual
            var miUltimaOferta = s.Pujas
                .Where(p => p.CompradorId == query.UsuarioId)
                .OrderByDescending(p => p.FechaPuja)
                .First()
                .Monto;

            // El líder actual de toda la subasta (la puja más alta, sea mía o de otro)
            var pujaLider = s.Pujas.OrderByDescending(p => p.Monto).First();

            return new MiPujaDto
            {
                SubastaId = s.Id,
                TituloSubasta = s.Titulo,
                MiUltimaOferta = miUltimaOferta,
                EstadoSubasta = s.Estado,
                SoyElLider = pujaLider.CompradorId == query.UsuarioId
            };
        }).ToList();
    }
}