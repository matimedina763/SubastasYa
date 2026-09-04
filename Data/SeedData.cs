using MiProyectoAPI.Models;
namespace MiProyectoAPI.Data;

public static class SeedData
{
    public static void IniciarDatosSemilla(AppDbContext context)
    {
        if(context.Usuarios.Any())
        {
            return; 
        }
    
    // Los datos semilla, a continuación:
    // Usuarios
    var vendedor = new Usuario { Email = "vendedor@test.com", Nombre = "Vendedor"};
    var comprador1 = new Usuario { Email = "comprador1@test.com", Nombre = "Comprador 1"};
    var comprador2 = new Usuario { Email = "comprador2@test.com", Nombre = "Comprador 2"};
    var sin_fondos = new Usuario { Email = "sinfondos@test.com", Nombre = "Sin Fondos"};
    context.Usuarios.AddRange(vendedor, comprador1, comprador2, sin_fondos);
    context.SaveChanges();

    // Billeteras
    var billetera_vendedor = new Billetera { UsuarioId = 1, SaldoTotal = 0, SaldoRetenido = 0};
    var billetera_comprador1 = new Billetera { UsuarioId = 2, SaldoTotal = 100000, SaldoRetenido = 45000 };
    var billetera_comprador2 = new Billetera { UsuarioId = 3, SaldoTotal = 100000, SaldoRetenido = 0};
    var billetera_sinsaldo = new Billetera { UsuarioId = 4, SaldoTotal = 1000, SaldoRetenido = 0 };  
    context.Billeteras.AddRange(billetera_vendedor, billetera_comprador1, billetera_comprador2, billetera_sinsaldo);
    context.SaveChanges();

    // Categorías
    var tecnologia = new Categoria { Nombre = "Tecnologia", Url_icono = "tec.png"};
    var coleccionables = new Categoria { Nombre = "Coleccionables", Url_icono = "col.png"};
    var indumentaria = new Categoria { Nombre = "Indumentaria", Url_icono = "ind.png"};
    var vehiculos = new Categoria { Nombre = "Vehiculos", Url_icono = "veh.png"};
    context.Categorias.AddRange(tecnologia, coleccionables, indumentaria, vehiculos);
    context.SaveChanges();

    // Subastas
    var subasta1 = new Subasta { Titulo = "Notebook Lenovo", Descripcion = "Original", precio_base = 400000, fecha_inicio = DateTime.Now, fecha_fin = DateTime.Now.AddDays(7), VendedorId = vendedor.Id, CategoriaId = tecnologia.Id, estado = "Activa"};
    var subasta2 = new Subasta { Titulo = "Buzo Hombre Adidas", Descripcion = "Original", precio_base = 110000, fecha_inicio = DateTime.Now, fecha_fin = DateTime.Now.AddDays(5), VendedorId = vendedor.Id, CategoriaId = vehiculos.Id, estado = "Activa"};
    context.Subastas.AddRange(subasta1, subasta2);
    context.SaveChanges();

    // Pujas y Transacciones en Ledger 
    var puja1 = new Puja { Monto = 45000, FechaPuja = DateTime.Now, CompradorId = comprador1.Id, SubastaId = subasta1.Id};
    context.Pujas.Add(puja1);
    context.SaveChanges();

    var ledger1 = new TransaccionLedger { BilleteraId = billetera_comprador1.Id, Monto = -45000, Tipo = "Retención"};
    context.TransaccionLedgers.Add(ledger1);
    context.SaveChanges();

    }
}    
