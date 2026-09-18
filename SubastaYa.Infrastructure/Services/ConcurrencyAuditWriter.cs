using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Domain.Entities;
using SubastaYa.Infrastructure.Data;

namespace SubastaYa.Infrastructure.Services;

public class ConcurrencyAuditWriter : IConcurrencyAuditWriter
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public ConcurrencyAuditWriter(
        IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task RegistrarAsync(string ruta, string metodo)
    {
        await using var context =
            await _contextFactory.CreateDbContextAsync();

        context.AuditoriaLogs.Add(
            new AuditoriaLog
            {
                Entidad = "PUJA",
                EntidadId = 0,
                Accion = "PUJA_RECHAZADA_CONCURRENCIA",
                Fecha = DateTime.UtcNow,
                DetalleJson = JsonSerializer.Serialize(
                    new
                    {
                        ruta,
                        metodo,
                        motivo = "DbUpdateConcurrencyException"
                    })
            });

        await context.SaveChangesAsync();
    }
}