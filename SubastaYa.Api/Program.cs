using Microsoft.EntityFrameworkCore;                // En teoría, no puede haber referencias de EF Core en la capa de Presentación. Se justifica registrando ISubastaRepository. (Composition root).
using SubastaYa.Api.Middleware;
using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Application.UseCases.Billeteras.DepositarSaldo;
using SubastaYa.Application.UseCases.Billeteras.ObtenerMovimientos;
using SubastaYa.Application.UseCases.Billeteras.ObtenerSaldo;
using SubastaYa.Application.UseCases.Subastas.CerrarSubastasVencidas;
using SubastaYa.Application.UseCases.Subastas.CrearSubasta;
using SubastaYa.Application.UseCases.Subastas.ListarSubastas;
using SubastaYa.Application.UseCases.Subastas.MisPublicaciones;
using SubastaYa.Application.UseCases.Subastas.MisPujas;
using SubastaYa.Application.UseCases.Subastas.ObtenerSubasta;  // AGREGUE: EL NAMESPACE DE ObtenerSubastaHandler
using SubastaYa.Application.UseCases.Subastas.RegistrarPuja;
using SubastaYa.Application.UseCases.Subastas.HistorialPujas;
using SubastaYa.Application.UseCases.Subastas.EstadoPuja;
using SubastaYa.Infrastructure.Data;
using SubastaYa.Infrastructure.Repositories;
using SubastaYa.Infrastructure.Services;
using SubastaYa.Infrastructure.Workers;




var builder = WebApplication.CreateBuilder(args);

// MODULO 1 - ¿QUE SERVICIOS EXISTEN? : Se registran en el contenedor de DI.   Todavía no corre nada.

builder.Services.AddEndpointsApiExplorer();                     // Habilita que .NET pueda describir los endpoints de la API (necesario para que Swagger los detecte)
builder.Services.AddSwaggerGen();                                // Genera automáticamente la documentación OpenAPI/Swagger de todos los endpoints
builder.Services.AddControllers();                               // Habilita el uso de Controllers (busca clases con [ApiController] y las conecta a las rutas HTTP)

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<AppDbContext>(sp =>
    sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

builder.Services.AddScoped<ISubastaRepository, SubastaRepository>();       // Cuando alguien pida ISubastaRepository, dale una instancia real de SubastaRepository (implementación con EF Core)
builder.Services.AddScoped<IBilleteraRepository, BilleteraRepository>();   // Ídem, para las consultas/movimientos de billeteras
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();                     // Ídem, para poder guardar todos los cambios juntos de forma atómica

builder.Services.AddScoped<ObtenerSubastaHandler>();              // Registra el Handler que resuelve la consulta de una subasta por id
builder.Services.AddScoped<RegistrarPujaCommandHandler>();        // Registra el Handler que ejecuta la lógica de negocio de registrar una puja (escrow + anti-sniping)

builder.Services.AddScoped<IAuditoriaLogRepository, AuditoriaLogRepository>();     // Cuando alguien pida IAuditoriaLogRepository, dale una instancia real de AuditoriaLogRepository (implementación con EF Core)
builder.Services.AddScoped<CerrarSubastasVencidasCommandHandler>();               // Registra el Handler que cierra subastas vencidas (adjudica ganador o pasa a DESIERTA)

builder.Services.AddHostedService<CierreSubastasWorker>();                        // Registra el Worker como un servicio de fondo: .NET lo arranca solo al iniciar la app y lo mantiene corriendo (ExecuteAsync en loop) durante toda su vida útil, sin que nadie lo invoque manualmente

builder.Services.AddScoped<ListarSubastasQueryHandler>();   // Registra el Handler que lista subastas con filtros opcionales (estado, categoria, rango de precio) y ordenamiento; se crea una instancia nueva por cada request HTTP
builder.Services.AddScoped<CrearSubastaCommandHandler>();   // Registra el Handler que crea subastas; se crea una instancia nueva por cada request HTTP

builder.Services.AddScoped<ObtenerSaldoQueryHandler>();          // Registra el Handler que consulta el saldo (Total/Retenido/Disponible) de la billetera de un usuario
builder.Services.AddScoped<DepositarSaldoCommandHandler>();      // Registra el Handler que acredita saldo simulado a una billetera y lo deja asentado en el Ledger
builder.Services.AddScoped<ObtenerMovimientosQueryHandler>();    // Registra el Handler que trae el historial de movimientos (depositos, retenciones, liberaciones, pagos, cobros) de la billetera de un usuario

builder.Services.AddScoped<MisPujasQueryHandler>();            // Registra el Handler que trae las subastas donde un usuario pujó, indicando su última oferta, si va liderando y el estado de cada subasta
builder.Services.AddScoped<MisPublicacionesQueryHandler>();    // Registra el Handler que trae las subastas publicadas por un vendedor, con cantidad de pujas y monto recaudado si ya se vendió

builder.Services.AddScoped<IConcurrencyAuditWriter, ConcurrencyAuditWriter>();  // Registra la auditoria del conflicto de concurrencia 409

builder.Services.AddScoped<HistorialPujasQueryHandler>();      // Registra el handler que trae el historial de pujas al contenedor de dependencias
builder.Services.AddScoped<EstadoPujaQueryHandler>();          

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5500", "http://127.0.0.1:5500")
                .AllowAnyHeader()
                .AllowAnyMethod();
    });
});

// Desde está línea para abajo no adiciono más tools, solo configurar el comportamiento de la API.
var app = builder.Build();

app.UseCors("frontend");  // Permite que el frontend pueda hacer llamadas a la API.  Se configura en Program.cs.

app.UseMiddleware<ExceptionMiddleware>();  // Captura las excepciones y devuelve un error 500 con el mensaje de la excepción.  Se puede mejorar para que devuelva un error 400 si es una excepción de negocio.  Se puede mejorar para que devuelva un error 404 si es una excepción de no encontrado.  Se puede mejorar para que devuelva un error 401 si es una excepción de no autorizado.  Se puede mejorar para que devuelva un error 403 si es una excepción de no permitido.  Se puede mejorar para que devuelva un error 409 si es una excepción de conflicto.  Se puede mejorar para que devuelva un error 422 si es una excepción de validación.  Se puede mejorar para que devuelva un error 429 si es una excepción de demasiadas solicitudes.  Se puede mejorar para que devuelva un error 503 si es una excepción de servicio no disponible.  Se puede mejorar para que devuelva un error 504 si es una excepción de tiempo de espera agotado.  Se puede mejorar para que devuelva un error 505 si es una excepción de versión no soportada.  Se puede mejorar para que devuelva un error 511 si es una excepción de autenticación requerida.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();                                         // Obliga a que todo entre por https.
app.MapControllers();                                              // Activa los paths (rutas) de los controladores API.

using (var scope = app.Services.CreateScope())
{
    var Services = scope.ServiceProvider;
    var contexto = Services.GetRequiredService<AppDbContext>();
    contexto.Database.Migrate();                                   // Crea la base de datos si no existe.
    SeedData.IniciarDatosSemilla(contexto);                        // Llena las cuatro categorías, usuarios y billeteras.
}

app.Run();                                                         // Ponemos a arrancar la API.


