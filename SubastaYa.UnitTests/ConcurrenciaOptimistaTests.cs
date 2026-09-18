using Microsoft.EntityFrameworkCore;
using SubastaYa.Infrastructure.Data;
using Xunit;

namespace SubastaYa.UnitTests;

public class ConcurrenciaOptimistaTests
{
    [Fact]
    public async Task DosContextosModificandoLaMismaSubasta_SegundoGuardadoLanzaDbUpdateConcurrencyException()
    {
        // Apuntamos ambos contextos al MISMO archivo de base de datos real
        var connectionString = "Data Source=C:/dev/SubastasYa/SubastaYa.Api/subastas.db";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connectionString)
            .Options;

        await using var contexto1 = new AppDbContext(options);
        await using var contexto2 = new AppDbContext(options);

        var subasta1 = await contexto1.Subastas.FirstAsync(s => s.Id == 6);
        var subasta2 = await contexto2.Subastas.FirstAsync(s => s.Id == 6);

        // Sesión 1 modifica y guarda PRIMERO -> esto sube el Version en la base
        subasta1.PrecioBase += 1;
        subasta1.Version++;
        await contexto1.SaveChangesAsync();

        // Sesión 2 todavía tiene el Version VIEJO en memoria
        subasta2.PrecioBase += 1;
        subasta2.Version++;

        // Acá debe explotar: la base ya no tiene el Version que Sesión 2 cree que tiene
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => contexto2.SaveChangesAsync());
    }
}