# SubastaYa

Plataforma web de subastas en tiempo real con comercio electrónico, desarrollada
como Trabajo Práctico de la cátedra Proyecto de Software.

## Stack tecnológico

- **Backend**: C# / ASP.NET Core Web API (.NET 8), arquitectura en 4 capas
  (Domain, Application, Infrastructure, Api) siguiendo Clean Architecture.
- **Base de datos**: SQLite, EF Core (Code-First con migraciones).
- **Frontend**: HTML / CSS / JavaScript (Vanilla) + Bootstrap 5.
- **Testing**: xUnit + NSubstitute.

## Estructura del proyecto

```
SubastaYa.sln
├── SubastaYa.Domain          (entidades, reglas de negocio, excepciones)
├── SubastaYa.Application     (casos de uso: Commands/Queries + Handlers)
├── SubastaYa.Infrastructure  (EF Core, repositorios, Worker de fondo)
├── SubastaYa.Api             (Controllers, Program.cs, Middleware)
├── SubastaYa.UnitTests       (tests unitarios)
└── frontend/                 (HTML/CSS/JS)
```

## Cómo levantar el proyecto

### Requisitos
- .NET SDK 8.0
- Visual Studio 2022 (o VS Code)

### Pasos

1. Clonar el repositorio:
   ```
   git clone <url-del-repo>
   ```

2. Verificar la herramienta de EF Core:
   ```
   dotnet ef --version
   ```
   Si no da `8.0.11`, instalarla:
   ```
   dotnet tool install --global dotnet-ef --version 8.0.11
   ```

3. Aplicar las migraciones (crea la base `subastas.db` con las 7 tablas):
   ```
   dotnet ef database update --project SubastaYa.Infrastructure --startup-project SubastaYa.Api
   ```

4. Correr la API (desde Visual Studio, F5, o por consola):
   ```
   dotnet run --project SubastaYa.Api
   ```
   Al arrancar, se ejecutan automáticamente las migraciones pendientes y el
   `SeedData` (4 usuarios, 4 categorías, 5 subastas de prueba).

5. Abrir Swagger: `https://localhost:7006/swagger`

6. Para el frontend: abrir la carpeta `frontend/` con la extensión Live Server
   de VS Code (o cualquier servidor estático), apuntando a `index.html`.
   El `API_URL` en los `.js` del frontend está configurado para
   `https://localhost:7006/api`.

### Usuarios semilla (para pruebas)

| Email | Rol | Saldo inicial |
|---|---|---|
| vendedor@test.com | Creador de publicaciones | $0 |
| comprador1@test.com | Postor líder | $150.000 |
| comprador2@test.com | Postor habilitado | $200.000 |
| sinfondos@test.com | Sin saldo (para probar rechazos) | $500 |

## Reglas de negocio implementadas

- **Escrow**: toda puja retiene el saldo del postor; al ser superado, se
  libera automáticamente en el mismo bloque transaccional.
- **Anti-sniping**: si una puja llega dentro de los últimos 60 segundos, la
  subasta extiende su cierre 2 minutos.
- **Worker en segundo plano**: revisa cada 30 segundos las subastas vencidas
  y las cierra automáticamente (liquidando al ganador o pasando a DESIERTA).
- **Auditoría**: se registran en `AuditoriaLog` los cambios de estado,
  extensiones por anti-sniping, y pujas rechazadas (por regla de negocio o
  por concurrencia).

## Prueba de concurrencia (Optimistic Locking)

El sistema usa un campo `Version` en `Subasta` y `Billetera` para optimistic
locking. Si dos operaciones modifican la misma fila con el `Version`
desactualizado, EF Core lanza `DbUpdateConcurrencyException`, que el
`ExceptionMiddleware` traduce a **409 Conflict**.

### Cómo se probó

Test determinístico en `SubastaYa.UnitTests/ConcurrenciaOptimistaTests.cs`:
abre dos `AppDbContext` distintos sobre el mismo archivo de base de datos,
ambos leen la misma fila con el mismo `Version`. El primero guarda con éxito
(el `Version` avanza en la base); el segundo, que todavía tiene el `Version`
viejo en memoria, lanza `DbUpdateConcurrencyException` al intentar guardar
— confirmando el mecanismo sin depender de timing de red.

Correrlo: Test Explorer → `ConcurrenciaOptimistaTests` → Run (con la API
detenida, ya que SQLite no permite escritura concurrente entre procesos).

## Tests unitarios

`SubastaYa.UnitTests` cubre, con mocks (NSubstitute):
- `RegistrarPujaCommandHandler`: puja válida, monto insuficiente, subasta no
  activa, fondos insuficientes, vendedor pujando en su propia subasta,
  extensión por anti-sniping.
- `ConcurrenciaOptimistaTests`: optimistic locking (ver sección anterior).

Correr todos: Test Explorer → Run All Tests.

## Endpoints principales

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/subastas` | Listado con filtros (`estado`, `categoriaId`, `precioMin`, `precioMax`, `ordenarPor`) |
| GET | `/api/subastas/{id}` | Detalle de una subasta |
| POST | `/api/subastas` | Crear una subasta |
| POST | `/api/subastas/{id}/pujas` | Registrar una puja |
| GET | `/api/subastas/{id}/pujas` | Historial de pujas de una subasta |
| GET | `/api/subastas/{id}/estado-puja?compradorId=` | Estado de la puja del usuario (liderando/superado) |
| GET | `/api/billeteras/{usuarioId}` | Consultar saldo |
| POST | `/api/billeteras/{usuarioId}/depositos` | Depositar saldo simulado |
| GET | `/api/billeteras/{usuarioId}/movimientos` | Historial de movimientos |
| GET | `/api/usuarios/{usuarioId}/pujas` | Mis compras/pujas |
| GET | `/api/usuarios/{usuarioId}/publicaciones` | Mis publicaciones |

## Integrantes

- [Nombre 1]
- [Nombre 2]
