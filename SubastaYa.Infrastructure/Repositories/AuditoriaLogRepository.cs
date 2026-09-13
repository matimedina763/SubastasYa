using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Domain.Entities;
using SubastaYa.Infrastructure.Data;

namespace SubastaYa.Infrastructure.Repositories;

public class AuditoriaLogRepository : IAuditoriaLogRepository
{
    private readonly AppDbContext _dbContext;

    public AuditoriaLogRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Agregar(AuditoriaLog log)
    {
        _dbContext.AuditoriaLogs.Add(log);
    }
}