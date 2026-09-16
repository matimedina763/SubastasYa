using Microsoft.EntityFrameworkCore; //Primero, no tenía EF Core.  Lo instalé pero resulta que tanto .NET como EF deben tener la misma versión.  Entonces, lo que hice fue instalar la versión 8 de EF para que lo haga con el .NET 8.
using SubastaYa.Domain.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;


namespace SubastaYa.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<TransaccionLedger> TransaccionLedgers => Set<TransaccionLedger>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Subasta> Subastas => Set<Subasta>();
    public DbSet<AuditoriaLog> AuditoriaLogs => Set<AuditoriaLog>();
    public DbSet<Billetera> Billeteras => Set<Billetera>();
    public DbSet<Puja> Pujas => Set<Puja>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var conversorUtc = new ValueConverter<DateTime, DateTime>(
            v => v,                                          // al guardar: no se toca nada
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));  // al leer: "estampa" que es UTC

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(conversorUtc);
                }
            }
        }

        // ... tus configuraciones existentes de Version/ConcurrencyCheck, que quedan igual, después de esto
    }
}
