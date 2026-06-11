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

    public string? ClientType { get; set; }
    public string? ClientCode { get; set; }
    public string? PassportNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Occupation { get; set; }
    public string? Nationality { get; set; }
    public string? TradeLicense { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? TaxVatNumber { get; set; }
    public string? AuthorizedRepresentative { get; set; }
    public string? Tags { get; set; }
    public string? RiskLevel { get; set; }
    public string? ClientCategory { get; set; }
    public string? BillingPreference { get; set; }
    public string? PaymentTerms { get; set; }
    public decimal? CreditLimit { get; set; }
    public string? PreferredContactMethod { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? SecondaryPhone { get; set; }
    public string? EmergencyContact { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsBlacklisted { get; set; }

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public Guid ChamberId { get; set; }
    public Chamber Chamber { get; set; } = null!;
    public ICollection<ClientCase> ClientCases { get; set; } = new List<ClientCase>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
