using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.UseCases.Billetera.ObtenerSaldo;
using SubastaYa.Application.UseCases.Billeteras.DepositarSaldo;
using SubastaYa.Application.UseCases.Billeteras.ObtenerMovimientos;
using SubastaYa.Application.UseCases.Billeteras.ObtenerSaldo;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BilleterasController : ControllerBase
    {
        private readonly ObtenerSaldoQueryHandler _obtenerSaldoHandler;
        private readonly DepositarSaldoCommandHandler _depositarSaldoHandler;
        private readonly ObtenerMovimientosQueryHandler _obtenerMovimientosHandler;

        public BilleterasController(
            ObtenerSaldoQueryHandler obtenerSaldoHandler,
            DepositarSaldoCommandHandler depositarSaldoHandler,
            ObtenerMovimientosQueryHandler obtenerMovimientosHandler)
        {
            _obtenerSaldoHandler = obtenerSaldoHandler;
            _depositarSaldoHandler = depositarSaldoHandler;
            _obtenerMovimientosHandler = obtenerMovimientosHandler;
        }

        [HttpGet("{usuarioId}")]
        public async Task<IActionResult> ObtenerSaldo(int usuarioId)
        {
            var resultado = await _obtenerSaldoHandler.Handle(new ObtenerSaldoQuery(usuarioId));
            return Ok(resultado);
        }

        [HttpPost("{usuarioId}/depositos")]
        public async Task<IActionResult> Depositar(int usuarioId, [FromBody] DepositarSaldoRequest request)
        {
            var command = new DepositarSaldoCommand { UsuarioId = usuarioId, Monto = request.Monto };
            await _depositarSaldoHandler.Handle(command);
            return Ok();
        }

        [HttpGet("{usuarioId}/movimientos")]
        public async Task<IActionResult> ObtenerMovimientos(int usuarioId)
        {
            var resultado = await _obtenerMovimientosHandler.Handle(new ObtenerMovimientosQuery(usuarioId));
            return Ok(resultado);
        }
    }
}