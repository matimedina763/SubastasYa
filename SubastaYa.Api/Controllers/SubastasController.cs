using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.UseCases.Subastas.ObtenerSubasta;
using SubastaYa.Application.UseCases.Subastas.RegistrarPuja;
using SubastaYa.Application.UseCases.Subastas.ListarSubastas;
using SubastaYa.Application.UseCases.Subastas.CrearSubasta;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubastasController : ControllerBase
    {
        private readonly ObtenerSubastaHandler _obtenerSubastaHandler;
        private readonly RegistrarPujaCommandHandler _registrarPujaHandler;
        private readonly ListarSubastasQueryHandler _listarSubastasHandler;
        private readonly CrearSubastaCommandHandler _crearSubastaHandler;
        public SubastasController(
            ObtenerSubastaHandler obtenerSubastaHandler,
            RegistrarPujaCommandHandler registrarPujaHandler,
            ListarSubastasQueryHandler listarSubastasHandler,
            CrearSubastaCommandHandler crearSubastaHandler)
        {
            _obtenerSubastaHandler = obtenerSubastaHandler;
            _registrarPujaHandler = registrarPujaHandler;
            _listarSubastasHandler = listarSubastasHandler;
            _crearSubastaHandler = crearSubastaHandler;
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

        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] ListarSubastasQuery query)
        {
            var resultado = await _listarSubastasHandler.Handle(query);
            return Ok(resultado);
        }

        [HttpPost]
        public async Task<IActionResult> CrearSubasta([FromBody] CrearSubastaCommand command)
        {
            var subastaId = await _crearSubastaHandler.Handle(command);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = subastaId }, new { subastaId });
        }        
    }
}