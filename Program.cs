using Microsoft.EntityFrameworkCore;
using MiProyectoAPI.Data;

var builder = WebApplication.CreateBuilder(args);  // Crear el constructor de la API. Es el que va a armar todo.

// Agrega servicios al contenedor.
// Aprende más acerca de configurar OpenAPI en https://aka.ms/aspnet/openapi
// "(builder.Services)" significa que necesitas herramientas como Swagger,
// controladores y Bases de datos.
builder.Services.AddEndpointsApiExplorer();  // Swagger puede descubrir que endpoints posees.
builder.Services.AddSwaggerGen();   // Este método abre Swagger para ejecutar los endpoints de la API.
builder.Services.AddControllers();  // Este método 
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));   // Primero usa el contexto, luego que use el motor SQLite y en GetConnectionStrings que busque la dirección en appsettings.json

// Construís estos servicios con todo lo que se pidió arriba.  Desde está línea para abajo no adiciono más tools, solo configurar el comportamiento de la API.
var app = builder.Build();

// Para ver Swagger mientras codeas en este entorno.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();   // Obliga a que todo entre por https.

app.MapControllers();    // Activa los paths (rutas) de los controladores API.
app.Run();          // Ponemos a arrancar la API.
//Sin estos dos últimos métodos, la API compila pero no hace nada.
