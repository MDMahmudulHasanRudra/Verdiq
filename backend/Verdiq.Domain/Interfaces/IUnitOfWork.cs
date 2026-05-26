namespace Verdiq.Domain.Interfaces;

public interface IUnitOfWork
{
    IGenericRepository<T> Repository<T>() where T : class;
    Task<int> CompleteAsync();
}
