using Verdiq.Domain.Entities;
using Task = System.Threading.Tasks.Task;

namespace Verdiq.Domain.Interfaces;

public interface IUnitOfWork
{
    IGenericRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> CompleteAsync();
}
