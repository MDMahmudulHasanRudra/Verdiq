namespace Verdiq.Domain.Entities;

public class Client : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? AssignedLawyerId { get; set; }
    public Guid OrganizationId { get; set; }

    public User? AssignedLawyer { get; set; }
    public Organization Organization { get; set; } = null!;
    public ICollection<Case> Cases { get; set; } = new List<Case>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
