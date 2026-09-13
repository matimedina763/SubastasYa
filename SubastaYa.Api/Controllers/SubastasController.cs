using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.UseCases.Subastas.ObtenerSubasta;
using SubastaYa.Application.UseCases.Subastas.RegistrarPuja;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubastasController : ControllerBase
    {
        private readonly ObtenerSubastaHandler _obtenerSubastaHandler;
        private readonly RegistrarPujaCommandHandler _registrarPujaHandler;

        public SubastasController(
            ObtenerSubastaHandler obtenerSubastaHandler,
            RegistrarPujaCommandHandler registrarPujaHandler)
        {
            _obtenerSubastaHandler = obtenerSubastaHandler;
            _registrarPujaHandler = registrarPujaHandler;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var resultado = await _obtenerSubastaHandler.Handle(new ObtenerSubastaQuery(id));
            return Ok(resultado);
        }

        [HttpPost("{id}/pujas")]
        public async Task<IActionResult> RegistrarPuja(int id, [FromBody] RegistrarPujaRequest request)
        {
            var command = new RegistrarPujaCommand(id, request.CompradorId, request.Monto);
            var pujaId = await _registrarPujaHandler.Handle(command);
            return CreatedAtAction(nameof(ObtenerPorId), new { id }, new { pujaId });
        }
    }
}