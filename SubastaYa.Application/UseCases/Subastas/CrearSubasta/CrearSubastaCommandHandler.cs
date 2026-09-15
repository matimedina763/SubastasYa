using SubastaYa.Domain.Exceptions;
using SubastaYa.Application.Interfaces.Persistence;

namespace SubastaYa.Application.UseCases.Subastas.CrearSubasta;

public class CrearSubastaCommandHandler
{
    private readonly ISubastaRepository _subastaRepository;

    public CrearSubastaCommandHandler(ISubastaRepository subastaRepository)
    {
        _subastaRepository = subastaRepository;
    }
    public async Task<int> Handle(CrearSubastaCommand command)
    {
        if (!command.PrecioBase.HasValue || command.PrecioBase.Value <= 0)
        {
            throw new DatosSubastaInvalidosException("El precio base es obligatorio y debe ser positivo.");
        }

        if (!command.IncrementoMinimo.HasValue || command.IncrementoMinimo.Value <= 0)
        {
            throw new DatosSubastaInvalidosException("El incremento mínimo es obligatorio y debe ser positivo.");
        }

        if (!command.FechaInicio.HasValue || !command.FechaFin.HasValue || command.FechaFin <= command.FechaInicio)
        {
            throw new DatosSubastaInvalidosException("Las fechas de inicio y fin son obligatorias y la fecha de fin debe ser posterior a la fecha de inicio.");
        }

        var subasta = new Domain.Entities.Subasta
        {
            VendedorId = command.VendedorId,
            Titulo = command.Titulo,
            Descripcion = command.Descripcion,
            UrlImagen = command.UrlImagen,
            CategoriaId = command.CategoriaId,
            PrecioBase = command.PrecioBase,
            IncrementoMinimo = command.IncrementoMinimo,
            FechaInicio = command.FechaInicio,
            FechaFin = command.FechaFin
        };

        await _subastaRepository.AgregarAsync(subasta);
        return subasta.Id;
    }
}