using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.UseCases.Subastas.MisPujas;
using SubastaYa.Application.UseCases.Subastas.MisPublicaciones;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly MisPujasQueryHandler _misPujasHandler;
        private readonly MisPublicacionesQueryHandler _misPublicacionesHandler;

        public UsuariosController(
            MisPujasQueryHandler misPujasHandler,
            MisPublicacionesQueryHandler misPublicacionesHandler)
        {
            _misPujasHandler = misPujasHandler;
            _misPublicacionesHandler = misPublicacionesHandler;
        }

        [HttpGet("{usuarioId}/pujas")]
        public async Task<IActionResult> MisPujas(int usuarioId)
        {
            var resultado = await _misPujasHandler.Handle(new MisPujasQuery(usuarioId));
            return Ok(resultado);
        }

        [HttpGet("{usuarioId}/publicaciones")]
        public async Task<IActionResult> MisPublicaciones(int usuarioId)
        {
            var resultado = await _misPublicacionesHandler.Handle(new MisPublicacionesQuery(usuarioId));
            return Ok(resultado);
        }
    }
}