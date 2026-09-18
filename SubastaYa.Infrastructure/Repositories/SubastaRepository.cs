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
            .Include(subasta => subasta.Categoria)
            .Include(subasta => subasta.Pujas)
                .ThenInclude(puja => puja.Comprador)  // ← nuevo
            .FirstOrDefaultAsync(subasta => subasta.Id == id);
    }

    public void AgregarPuja(Puja puja)
    {
        _dbContext.Pujas.Add(puja);
    }

    public async Task<List<Subasta>> ObtenerActivasVencidasAsync(DateTime ahora)
    {
        return await _dbContext.Subastas
            .Include(subasta => subasta.Pujas)
            .Where(subasta =>
                subasta.Estado == "ACTIVA" &&
                subasta.FechaFin <= ahora)
            .ToListAsync();
    }

    public async Task<List<Subasta>> ListarAsync(
    string? estado,
    int? categoriaId,
    decimal? precioMin,
    decimal? precioMax,
    string? ordenarPor)
{
    var consultaSubastas = _dbContext.Subastas
        .Include(subasta => subasta.Categoria)
        .Include(subasta => subasta.Pujas)
            .ThenInclude(puja => puja.Comprador)
        .AsQueryable();

    if (!string.IsNullOrEmpty(estado))
    {
        consultaSubastas = consultaSubastas
            .Where(subasta => subasta.Estado == estado);
    }

    if (categoriaId.HasValue)
    {
        consultaSubastas = consultaSubastas
            .Where(subasta =>
                subasta.CategoriaId == categoriaId.Value);
    }

    if (precioMin.HasValue)
    {
        consultaSubastas = consultaSubastas
            .Where(subasta =>
                subasta.PrecioBase >= precioMin.Value);
    }

    if (precioMax.HasValue)
    {
        consultaSubastas = consultaSubastas
            .Where(subasta =>
                subasta.PrecioBase <= precioMax.Value);
    }

    var subastas = await consultaSubastas.ToListAsync();

    subastas = ordenarPor switch
    {
        "MenorTiempoRestante" =>
            subastas
                .OrderBy(subasta => subasta.FechaFin)
                .ToList(),

        "MayorPuja" =>
            subastas
                .OrderByDescending(subasta =>
                    subasta.Pujas.Any()
                        ? subasta.Pujas.Max(puja => puja.Monto)
                        : subasta.PrecioBase)
                .ToList(),

        _ =>
            subastas
                .OrderBy(subasta => subasta.Id)
                .ToList()
    };

    return subastas;
}

    public void Agregar(Subasta subasta)
    {
        _dbContext.Subastas.Add(subasta);
    }
    public async Task<List<Subasta>> ObtenerSubastasConPujaDeUsuarioAsync(int usuarioId)
    {
        return await _dbContext.Subastas
            .Include(subasta => subasta.Pujas)
            .Where(subasta =>
                subasta.Pujas.Any(puja => puja.CompradorId == usuarioId))
            .ToListAsync();
    }

    public async Task<List<Subasta>> ObtenerPorVendedorIdAsync(int vendedorId)
    {
        return await _dbContext.Subastas
            .Include(subasta => subasta.Pujas)
            .Where(subasta => subasta.VendedorId == vendedorId)
            .ToListAsync();
    }
}