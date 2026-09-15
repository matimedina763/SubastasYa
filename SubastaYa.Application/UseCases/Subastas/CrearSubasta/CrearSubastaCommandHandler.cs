using SubastaYa.Domain.Entities;
using SubastaYa.Domain.Exceptions;
using SubastaYa.Application.Interfaces.Persistence;

namespace SubastaYa.Application.UseCases.Subastas.CrearSubasta;

public class CrearSubastaCommandHandler
{
    private readonly ISubastaRepository _subastaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearSubastaCommandHandler(ISubastaRepository subastaRepository, IUnitOfWork unitOfWork)
    {
        _subastaRepository = subastaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CrearSubastaCommand command)
    {
        if (command.PrecioBase <= 0)
            throw new DatosSubastaInvalidosException("El precio base es obligatorio y debe ser positivo.");

        if (command.IncrementoMinimo <= 0)
            throw new DatosSubastaInvalidosException("El incremento mínimo es obligatorio y debe ser positivo.");

        if (command.FechaFin <= command.FechaInicio)
            throw new DatosSubastaInvalidosException("La fecha de fin debe ser posterior a la fecha de inicio.");

        var estadoInicial = command.FechaInicio > DateTime.UtcNow ? "PROGRAMADA" : "ACTIVA";

        var subasta = new Subasta
        {
            VendedorId = command.VendedorId,
            Titulo = command.Titulo,
            Descripcion = command.Descripcion,
            UrlImagen = command.UrlImagen,
            CategoriaId = command.CategoriaId,
            PrecioBase = command.PrecioBase,
            IncrementoMinimo = command.IncrementoMinimo,
            FechaInicio = command.FechaInicio,
            FechaFin = command.FechaFin,
            Estado = estadoInicial
        };

        _subastaRepository.Agregar(subasta);
        await _unitOfWork.SaveChangesAsync();

        return subasta.Id;
    }
}