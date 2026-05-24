namespace Verdiq.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Entities.User> Users { get; }
    IGenericRepository<Entities.Client> Clients { get; }
    IGenericRepository<Entities.Case> Cases { get; }
    IGenericRepository<Entities.Hearing> Hearings { get; }
    IGenericRepository<Entities.Document> Documents { get; }
    IGenericRepository<Entities.Notification> Notifications { get; }
    IGenericRepository<Entities.Subscription> Subscriptions { get; }
    IGenericRepository<Entities.Payment> Payments { get; }
    IGenericRepository<Entities.AuditLog> AuditLogs { get; }

    Task<int> CompleteAsync();
}
