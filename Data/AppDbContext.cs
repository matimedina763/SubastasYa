using Microsoft.EntityFrameworkCore;
using MiProyectoAPI.Models;

namespace MiProyectoAPI.Data; 

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
}