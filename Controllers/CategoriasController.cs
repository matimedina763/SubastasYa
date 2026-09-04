using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiProyectoAPI.Data;

namespace MiProyectoAPI.Controllers
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
        public async Task<ActionResult> Get()
        {
            var Categorias = await _context.Categorias.ToListAsync();
            return Ok (Categorias);
        }
    }
}
