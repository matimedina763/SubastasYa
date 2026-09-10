using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;  //En teoría, no puede haber referencias de EF Core en la capa de Presentación. Se justifica registrando ISubastaRepository. (Composition root).
using SubastaYa.Infrastructure.Data;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class CategoriasController : ControllerBase
    {
        private readonly AppDbContext _context = null!;
        public CategoriasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var Categorias = await _context.Categorias.ToListAsync();
            return Ok (Categorias);
        }
    }
}
