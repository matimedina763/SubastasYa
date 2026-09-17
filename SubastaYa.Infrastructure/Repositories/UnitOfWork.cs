using SubastaYa.Application.Interfaces.Persistence;
using SubastaYa.Infrastructure.Data;

namespace SubastaYa.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
    {
        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var result = await operation();

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}