using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Domain.Entities;
using SubastaYa.Infrastructure.Data;

namespace SubastaYa.Infrastructure.Repositories;

public class BilleteraRepository : IBilleteraRepository
{
    private readonly AppDbContext _dbContext;

    public BilleteraRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Billetera?> ObtenerPorUsuarioIdAsync(int usuarioId)
    {
        return await _dbContext.Billeteras.FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);
    }

    public void AgregarMovimientoLedger(TransaccionLedger movimiento)
    {
        _dbContext.TransaccionLedgers.Add(movimiento);
    }

    public async Task<List<TransaccionLedger>> ObtenerMovimientosPorUsuarioIdAsync(int usuarioId)
    {
        var billetera = await _dbContext.Billeteras.FirstOrDefaultAsync(b => b.UsuarioId == usuarioId);
        if (billetera is null) return new List<TransaccionLedger>();

        return await _dbContext.TransaccionLedgers
            .Where(t => t.BilleteraId == billetera.Id)
            .ToListAsync();
    }
}