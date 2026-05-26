namespace Verdiq.Domain.Entities;

public class Client : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Nid { get; set; }
    public string? CompanyName { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;
    public ICollection<ClientCase> ClientCases { get; set; } = new List<ClientCase>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
