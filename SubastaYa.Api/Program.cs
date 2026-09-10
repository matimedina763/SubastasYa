using Microsoft.EntityFrameworkCore;                // En teoría, no puede haber referencias de EF Core en la capa de Presentación. Se justifica registrando ISubastaRepository. (Composition root).
using SubastaYa.Infrastructure.Data;
/*using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Infrastructure.Repositories;
using SubastaYa.Application.UseCases; */
var builder = WebApplication.CreateBuilder(args);   // Crear el constructor de la API. Es el que va a armar todo.


builder.Services.AddEndpointsApiExplorer();  
builder.Services.AddSwaggerGen();   
builder.Services.AddControllers();  
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));   // Primero usa el contexto, luego que use el motor SQLite y en GetConnectionStrings que busque la dirección en appsettings.json
/* builder.Services.AddScoped<ISubastaRepository, SubastaRepository>();  
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();  
builder.Services.AddScoped<CrearSubastaHandler>();  
builder.Services.AddScoped<ObtenerSubastasHandler>();   */

// Desde está línea para abajo no adiciono más tools, solo configurar el comportamiento de la API.
var app = builder.Build();

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


