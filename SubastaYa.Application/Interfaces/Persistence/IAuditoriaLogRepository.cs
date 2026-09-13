using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces.Persistence;

public interface IAuditoriaLogRepository
{
    void Agregar(AuditoriaLog log);
}