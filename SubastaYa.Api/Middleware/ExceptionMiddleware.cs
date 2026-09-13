using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context); // ejecuta el resto del pipeline (Controller, Handler, etc.)
        }
        catch (SubastaNoEncontradaException ex)
        {
            await EscribirRespuesta(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (FondosInsuficientesException ex)
        {
            await EscribirRespuesta(context, StatusCodes.Status422UnprocessableEntity, ex.Message);
        }
        catch (DomainException ex) // atrapa PujaInvalidaException, SubastaNoActivaException, y cualquier otra que no tenga un catch específico arriba
        {
            await EscribirRespuesta(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (DbUpdateConcurrencyException)
        {
            await EscribirRespuesta(context, StatusCodes.Status409Conflict,
                "La subasta fue modificada por otra operación concurrente. Intentá nuevamente.");
        }
        catch (Exception ex)
        {
            await EscribirRespuesta(context, StatusCodes.Status500InternalServerError,
                "Ocurrió un error interno inesperado.");
            // Acá, en un proyecto real, se loguearía "ex" con más detalle (Serilog, etc.)
        }
    }

    private static async Task EscribirRespuesta(HttpContext context, int statusCode, string mensaje)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var respuesta = JsonSerializer.Serialize(new { error = mensaje });
        await context.Response.WriteAsync(respuesta);
    }
}