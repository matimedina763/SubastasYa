using SubastaYa.Domain.Entities;
namespace SubastaYa.Infrastructure.Data;

public static class SeedData
{
    public static void IniciarDatosSemilla(AppDbContext context)
    {
        if (context.Usuarios.Any())
        {
            return;
        }

        // ── Usuarios ──────────────────────────────────────────────
        var vendedor = new Usuario { Email = "vendedor@test.com", Nombre = "Vendedor" };
        var comprador1 = new Usuario { Email = "comprador1@test.com", Nombre = "Comprador 1" };
        var comprador2 = new Usuario { Email = "comprador2@test.com", Nombre = "Comprador 2" };
        var sinFondos = new Usuario { Email = "sinfondos@test.com", Nombre = "Sin Fondos" };
        context.Usuarios.AddRange(vendedor, comprador1, comprador2, sinFondos);
        context.SaveChanges();

        // ── Billeteras ────────────────────────────────────────────
        // comprador1: líder en la subasta "Activa estándar" -> Retenido $45.000
        // comprador2: gana la subasta "Vencida con ganador" -> queda con Retenido pendiente de liquidar (Opción A)
        var billeteraVendedor = new Billetera { UsuarioId = vendedor.Id, SaldoTotal = 0, SaldoRetenido = 0 };
        var billeteraComprador1 = new Billetera { UsuarioId = comprador1.Id, SaldoTotal = 150000, SaldoRetenido = 45000 };
        var billeteraComprador2 = new Billetera { UsuarioId = comprador2.Id, SaldoTotal = 200000, SaldoRetenido = 50000 };
        var billeteraSinFondos = new Billetera { UsuarioId = sinFondos.Id, SaldoTotal = 500, SaldoRetenido = 0 };
        context.Billeteras.AddRange(billeteraVendedor, billeteraComprador1, billeteraComprador2, billeteraSinFondos);
        context.SaveChanges();

        // ── Categorías ────────────────────────────────────────────
        var tecnologia = new Categoria { Nombre = "Tecnologia", Url_icono = "tec.png" };
        var coleccionables = new Categoria { Nombre = "Coleccionables", Url_icono = "col.png" };
        var indumentaria = new Categoria { Nombre = "Indumentaria", Url_icono = "ind.png" };
        var vehiculos = new Categoria { Nombre = "Vehiculos", Url_icono = "veh.png" };
        context.Categorias.AddRange(tecnologia, coleccionables, indumentaria, vehiculos);
        context.SaveChanges();

        // ── Subastas: los 5 casos de prueba que pide el PDF ────────
        var subastaActivaEstandar = new Subasta
        {
            Titulo = "Notebook Lenovo",
            Descripcion = "Original, caja cerrada",
            PrecioBase = 30000,
            IncrementoMinimo = 1000,
            FechaInicio = DateTime.Now.AddDays(-1),
            FechaFin = DateTime.Now.AddMinutes(25), // cierra en 20-30 min
            VendedorId = vendedor.Id,
            CategoriaId = tecnologia.Id,
            Estado = "ACTIVA"
        };

        var subastaActivaCritica = new Subasta
        {
            Titulo = "Figura de colección",
            Descripcion = "Edición limitada",
            PrecioBase = 15000,
            IncrementoMinimo = 500,
            FechaInicio = DateTime.Now.AddDays(-1),
            FechaFin = DateTime.Now.AddSeconds(90), // cierra en menos de 2 min -> ventana anti-sniping
            VendedorId = vendedor.Id,
            CategoriaId = coleccionables.Id,
            Estado = "ACTIVA"
        };

        var subastaProxima = new Subasta
        {
            Titulo = "Campera de cuero",
            Descripcion = "Talle M",
            PrecioBase = 20000,
            IncrementoMinimo = 1000,
            FechaInicio = DateTime.Now.AddHours(24), // inicio programado a +24hs
            FechaFin = DateTime.Now.AddDays(8),
            VendedorId = vendedor.Id,
            CategoriaId = indumentaria.Id,
            Estado = "PROGRAMADA" // pujas bloqueadas
        };

        var subastaVencidaConGanador = new Subasta
        {
            Titulo = "Bicicleta de montaña",
            Descripcion = "Rodado 29",
            PrecioBase = 40000,
            IncrementoMinimo = 2000,
            FechaInicio = DateTime.Now.AddDays(-3),
            FechaFin = DateTime.Now.AddHours(-2), // ya venció
            VendedorId = vendedor.Id,
            CategoriaId = vehiculos.Id,
            Estado = "ACTIVA" // todavía no la procesó el Worker
        };

        var subastaVencidaDesierta = new Subasta
        {
            Titulo = "Consola retro",
            Descripcion = "Sin pujas recibidas",
            PrecioBase = 10000,
            IncrementoMinimo = 500,
            FechaInicio = DateTime.Now.AddDays(-3),
            FechaFin = DateTime.Now.AddHours(-1), // ya venció, nadie ofertó
            VendedorId = vendedor.Id,
            CategoriaId = tecnologia.Id,
            Estado = "ACTIVA" // todavía no la procesó el Worker
        };

        context.Subastas.AddRange(
            subastaActivaEstandar,
            subastaActivaCritica,
            subastaProxima,
            subastaVencidaConGanador,
            subastaVencidaDesierta
        );
        context.SaveChanges();

        // ── Pujas ─────────────────────────────────────────────────
        // 2 ofertas previas en la subasta activa estándar; líder final: comprador1 con $45.000
        var pujaPrevia = new Puja
        {
            Monto = 40000,
            FechaPuja = DateTime.Now.AddHours(-5),
            CompradorId = comprador2.Id,
            SubastaId = subastaActivaEstandar.Id
        };
        var pujaLider = new Puja
        {
            Monto = 45000,
            FechaPuja = DateTime.Now.AddHours(-1),
            CompradorId = comprador1.Id,
            SubastaId = subastaActivaEstandar.Id
        };

        // Puja ganadora (todavía no liquidada) en la subasta vencida con ganador
        var pujaGanadora = new Puja
        {
            Monto = 50000,
            FechaPuja = DateTime.Now.AddDays(-2),
            CompradorId = comprador2.Id,
            SubastaId = subastaVencidaConGanador.Id
        };

        context.Pujas.AddRange(pujaPrevia, pujaLider, pujaGanadora);
        context.SaveChanges();

        // ── Libro mayor (Ledger): depósitos + retenciones ──────────
        context.TransaccionLedgers.AddRange(
            // Depósitos iniciales, uno por cada billetera con saldo
            new TransaccionLedger { BilleteraId = billeteraComprador1.Id, Monto = 150000, Tipo = "DEPOSITO" },
            new TransaccionLedger { BilleteraId = billeteraComprador2.Id, Monto = 200000, Tipo = "DEPOSITO" },
            new TransaccionLedger { BilleteraId = billeteraSinFondos.Id, Monto = 500, Tipo = "DEPOSITO" },

            // Retención de comprador1 por liderar la subasta activa estándar
            new TransaccionLedger
            {
                BilleteraId = billeteraComprador1.Id,
                Monto = -45000,
                Tipo = "RETENCION",
                SubastaId = subastaActivaEstandar.Id
            },

            // Retención de comprador2 por ganar la subasta vencida (pendiente de liquidar por el Worker)
            new TransaccionLedger
            {
                BilleteraId = billeteraComprador2.Id,
                Monto = -50000,
                Tipo = "RETENCION",
                SubastaId = subastaVencidaConGanador.Id
            }
        );
        context.SaveChanges();
    }
}