using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.UseCases.Subastas.ObtenerSubasta;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubastasController : ControllerBase
    {
        private readonly ObtenerSubastaHandler _obtenerSubastaHandler;

        public SubastasController(ObtenerSubastaHandler obtenerSubastaHandler)
        {
            _obtenerSubastaHandler = obtenerSubastaHandler;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var resultado = await _obtenerSubastaHandler.Handle(new ObtenerSubastaQuery(id));
            return Ok(resultado);
        }
    }
}