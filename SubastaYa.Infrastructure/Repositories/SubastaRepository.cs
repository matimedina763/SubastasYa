using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Domain.Entities;
using SubastaYa.Infrastructure.Data;

namespace SubastaYa.Infrastructure.Repositories;

public class SubastaRepository : ISubastaRepository
{
    private readonly AppDbContext _dbContext;

    public SubastaRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Subasta?> ObtenerSubastaPorIdAsync(int id)
    {
        return await _dbContext.Subastas
            .Include(s => s.Pujas)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public void AgregarPuja(Puja puja)
    {
        _dbContext.Pujas.Add(puja);
    }
}