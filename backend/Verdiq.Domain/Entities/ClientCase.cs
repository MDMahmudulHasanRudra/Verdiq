namespace Verdiq.Domain.Entities;

public class ClientCase : BaseEntity
{
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public Guid CaseId { get; set; }
    public Case Case { get; set; } = null!;
}
