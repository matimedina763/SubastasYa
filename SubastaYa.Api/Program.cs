using Microsoft.EntityFrameworkCore;                // En teoría, no puede haber referencias de EF Core en la capa de Presentación. Se justifica registrando ISubastaRepository. (Composition root).
using SubastaYa.Infrastructure.Data;
using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Application.UseCases.Subastas.ObtenerSubasta;  // AGREGUE: EL NAMESPACE DE ObtenerSubastaHandler
using SubastaYa.Infrastructure.Repositories;


var builder = WebApplication.CreateBuilder(args);   

// MODULO 1 - ¿QUE SERVICIOS EXISTEN? : Se registran en el contenedor de DI.   Todavía no corre nada.
builder.Services.AddEndpointsApiExplorer();  
builder.Services.AddSwaggerGen();   
builder.Services.AddControllers();  
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));   // Primero usa el contexto, luego que use el motor SQLite y en GetConnectionStrings que busque la dirección en appsettings.json
builder.Services.AddScoped<ISubastaRepository, SubastaRepository>();  
builder.Services.AddScoped<ObtenerSubastaHandler>();          // AGREGUE: el servicio de ObtenerSubastaHandler


// Desde está línea para abajo no adiciono más tools, solo configurar el comportamiento de la API.
var app = builder.Build();

//app.UseMiddleware<ExceptionMiddleware>();  // Captura las excepciones y devuelve un error 500 con el mensaje de la excepción.  Se puede mejorar para que devuelva un error 400 si es una excepción de negocio.  Se puede mejorar para que devuelva un error 404 si es una excepción de no encontrado.  Se puede mejorar para que devuelva un error 401 si es una excepción de no autorizado.  Se puede mejorar para que devuelva un error 403 si es una excepción de no permitido.  Se puede mejorar para que devuelva un error 409 si es una excepción de conflicto.  Se puede mejorar para que devuelva un error 422 si es una excepción de validación.  Se puede mejorar para que devuelva un error 429 si es una excepción de demasiadas solicitudes.  Se puede mejorar para que devuelva un error 503 si es una excepción de servicio no disponible.  Se puede mejorar para que devuelva un error 504 si es una excepción de tiempo de espera agotado.  Se puede mejorar para que devuelva un error 505 si es una excepción de versión no soportada.  Se puede mejorar para que devuelva un error 511 si es una excepción de autenticación requerida.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();                                         // Obliga a que todo entre por https.
app.UseCors("frontend");                                           // Permite que el frontend pueda hacer llamadas a la API.  Se configura en Program.cs.
app.MapControllers();                                              // Activa los paths (rutas) de los controladores API.

using (var scope = app.Services.CreateScope())
{
    var Services = scope.ServiceProvider;
    var contexto = Services.GetRequiredService<AppDbContext>();
    contexto.Database.Migrate();                                   // Crea la base de datos si no existe.
    SeedData.IniciarDatosSemilla(contexto);                        // Llena las cuatro categorías, usuarios y billeteras.
}

app.Run();                                                         // Ponemos a arrancar la API.


