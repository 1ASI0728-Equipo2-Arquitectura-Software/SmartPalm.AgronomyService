namespace SmartPalm.AgronomyService.Domain.Repositories;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken token = default);
    Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken token);
}
