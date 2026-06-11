namespace Verdiq.Application.DTOs.Client;

public class CreateClientDto
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Nid { get; set; }
    public string? CompanyName { get; set; }
    public string? Notes { get; set; }
    public string? ClientType { get; set; }
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
}

public class UpdateClientDto
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Nid { get; set; }
    public string? CompanyName { get; set; }
    public string? Notes { get; set; }
    public string? ClientType { get; set; }
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
    public bool? IsBlacklisted { get; set; }
    public bool? IsActive { get; set; }
    public string? AvatarUrl { get; set; }
}

public class ClientResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Nid { get; set; }
    public string? CompanyName { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public int CasesCount { get; set; }
    public DateTime CreatedAt { get; set; }
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
    public bool IsBlacklisted { get; set; }
    public string? AvatarUrl { get; set; }
}
