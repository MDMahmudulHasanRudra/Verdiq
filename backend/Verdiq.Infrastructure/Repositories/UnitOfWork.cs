using Verdiq.Domain.Entities;
using Verdiq.Domain.Interfaces;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IGenericRepository<User>? _users;
    private IGenericRepository<Client>? _clients;
    private IGenericRepository<Case>? _cases;
    private IGenericRepository<Hearing>? _hearings;
    private IGenericRepository<Document>? _documents;
    private IGenericRepository<Notification>? _notifications;
    private IGenericRepository<Subscription>? _subscriptions;
    private IGenericRepository<Payment>? _payments;
    private IGenericRepository<AuditLog>? _auditLogs;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<User> Users =>
        _users ??= new GenericRepository<User>(_context);

    public IGenericRepository<Client> Clients =>
        _clients ??= new GenericRepository<Client>(_context);

    public IGenericRepository<Case> Cases =>
        _cases ??= new GenericRepository<Case>(_context);

    public IGenericRepository<Hearing> Hearings =>
        _hearings ??= new GenericRepository<Hearing>(_context);

    public IGenericRepository<Document> Documents =>
        _documents ??= new GenericRepository<Document>(_context);

    public IGenericRepository<Notification> Notifications =>
        _notifications ??= new GenericRepository<Notification>(_context);

    public IGenericRepository<Subscription> Subscriptions =>
        _subscriptions ??= new GenericRepository<Subscription>(_context);

    public IGenericRepository<Payment> Payments =>
        _payments ??= new GenericRepository<Payment>(_context);

    public IGenericRepository<AuditLog> AuditLogs =>
        _auditLogs ??= new GenericRepository<AuditLog>(_context);

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
