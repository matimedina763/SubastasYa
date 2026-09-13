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

    public async Task<List<Subasta>> ObtenerActivasVencidasAsync(DateTime ahora)
    {
        return await _dbContext.Subastas
            .Include(s => s.Pujas)
            .Where(s => s.Estado == "ACTIVA" && s.FechaFin <= ahora)
            .ToListAsync();
    }

    public async Task<List<Subasta>> ListarAsync(
    string? estado, int? categoriaId, decimal? precioMin, decimal? precioMax, string? ordenarPor)
    {
        var query = _dbContext.Subastas.Include(s => s.Pujas).AsQueryable();

        if (!string.IsNullOrEmpty(estado))
            query = query.Where(s => s.Estado == estado);

        if (categoriaId.HasValue)
            query = query.Where(s => s.CategoriaId == categoriaId.Value);

        if (precioMin.HasValue)
            query = query.Where(s => s.PrecioBase >= precioMin.Value);

        if (precioMax.HasValue)
            query = query.Where(s => s.PrecioBase <= precioMax.Value);

        // Los filtros (WHERE) sí viajan a SQL, sin problema -> se ejecutan en la base.
        var subastas = await query.ToListAsync();

        // El ORDENAMIENTO se hace ACÁ, en memoria, ya con la lista traída.
        // Esto es plain C#, no SQL -> nunca puede fallar por traducción.
        subastas = ordenarPor switch
        {
            "MenorTiempoRestante" => subastas.OrderBy(s => s.FechaFin).ToList(),
            "MayorPuja" => subastas.OrderByDescending(s =>
                s.Pujas.Any() ? s.Pujas.Max(p => p.Monto) : s.PrecioBase).ToList(),
            _ => subastas.OrderBy(s => s.Id).ToList()
        };

        return subastas;
    }
}