namespace SubastaYa.Application.Interfaces.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();

    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);
}