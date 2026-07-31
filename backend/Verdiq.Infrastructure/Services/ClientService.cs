using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.Client;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class ClientService : IClientService
{
    private readonly AppDbContext _context;

    public ClientService(AppDbContext context) => _context = context;

    public async Task<(bool Success, string Message, ClientResponseDto? Data)> CreateAsync(CreateClientDto dto, Guid chamberId)
    {
        var client = new Client
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            Nid = dto.Nid,
            CompanyName = dto.CompanyName,
            Notes = dto.Notes,
            ClientType = dto.ClientType,
            PassportNumber = dto.PassportNumber,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            Occupation = dto.Occupation,
            Nationality = dto.Nationality,
            TradeLicense = dto.TradeLicense,
            RegistrationNumber = dto.RegistrationNumber,
            TaxVatNumber = dto.TaxVatNumber,
            AuthorizedRepresentative = dto.AuthorizedRepresentative,
            Tags = dto.Tags,
            RiskLevel = dto.RiskLevel,
            ClientCategory = dto.ClientCategory,
            BillingPreference = dto.BillingPreference,
            PaymentTerms = dto.PaymentTerms,
            CreditLimit = dto.CreditLimit,
            PreferredContactMethod = dto.PreferredContactMethod,
            WhatsAppNumber = dto.WhatsAppNumber,
            SecondaryPhone = dto.SecondaryPhone,
            EmergencyContact = dto.EmergencyContact,
            ClientCode = await GenerateClientCode(chamberId),
            IsActive = true,
            ChamberId = chamberId,
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        var result = MapToDto(client);
        return (true, "Client created successfully", result);
    }

    public async Task<(bool Success, string Message, ClientResponseDto? Data)> UpdateAsync(Guid id, UpdateClientDto dto)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null || client.IsDeleted)
            return (false, "Client not found", null);

        if (dto.Name != null) client.Name = dto.Name;
        if (dto.Email != null) client.Email = dto.Email;
        if (dto.Phone != null) client.Phone = dto.Phone;
        if (dto.Address != null) client.Address = dto.Address;
        if (dto.Nid != null) client.Nid = dto.Nid;
        if (dto.CompanyName != null) client.CompanyName = dto.CompanyName;
        if (dto.Notes != null) client.Notes = dto.Notes;
        if (dto.ClientType != null) client.ClientType = dto.ClientType;
        if (dto.PassportNumber != null) client.PassportNumber = dto.PassportNumber;
        if (dto.DateOfBirth.HasValue) client.DateOfBirth = dto.DateOfBirth;
        if (dto.Gender != null) client.Gender = dto.Gender;
        if (dto.Occupation != null) client.Occupation = dto.Occupation;
        if (dto.Nationality != null) client.Nationality = dto.Nationality;
        if (dto.TradeLicense != null) client.TradeLicense = dto.TradeLicense;
        if (dto.RegistrationNumber != null) client.RegistrationNumber = dto.RegistrationNumber;
        if (dto.TaxVatNumber != null) client.TaxVatNumber = dto.TaxVatNumber;
        if (dto.AuthorizedRepresentative != null) client.AuthorizedRepresentative = dto.AuthorizedRepresentative;
        if (dto.Tags != null) client.Tags = dto.Tags;
        if (dto.RiskLevel != null) client.RiskLevel = dto.RiskLevel;
        if (dto.ClientCategory != null) client.ClientCategory = dto.ClientCategory;
        if (dto.BillingPreference != null) client.BillingPreference = dto.BillingPreference;
        if (dto.PaymentTerms != null) client.PaymentTerms = dto.PaymentTerms;
        if (dto.CreditLimit.HasValue) client.CreditLimit = dto.CreditLimit;
        if (dto.PreferredContactMethod != null) client.PreferredContactMethod = dto.PreferredContactMethod;
        if (dto.WhatsAppNumber != null) client.WhatsAppNumber = dto.WhatsAppNumber;
        if (dto.SecondaryPhone != null) client.SecondaryPhone = dto.SecondaryPhone;
        if (dto.EmergencyContact != null) client.EmergencyContact = dto.EmergencyContact;
        if (dto.IsBlacklisted.HasValue) client.IsBlacklisted = dto.IsBlacklisted.Value;
        if (dto.IsActive.HasValue) client.IsActive = dto.IsActive.Value;

        client.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var result = MapToDto(client);
        return (true, "Client updated successfully", result);
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null || client.IsDeleted)
            return (false, "Client not found");

        client.IsDeleted = true;
        client.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return (true, "Client deleted successfully");
    }

    public async Task<ClientResponseDto?> GetByIdAsync(Guid id)
    {
        var client = await _context.Clients
            .Include(c => c.ClientCases.Where(cc => !cc.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        return client == null ? null : MapToDto(client);
    }

    public async Task<IEnumerable<ClientResponseDto>> GetAllAsync(Guid chamberId, int page = 1, int pageSize = 10, string? search = null, string? status = null, string? clientType = null)
    {
        var clients = await BuildQuery(chamberId, search, status, clientType)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return clients.Select(MapToDto);
    }

    public async Task<IEnumerable<ClientResponseDto>> SearchAsync(string query, Guid chamberId)
    {
        var term = query.ToLower();
        var clients = await _context.Clients
            .Include(c => c.ClientCases.Where(cc => !cc.IsDeleted))
            .Where(c => c.ChamberId == chamberId && !c.IsDeleted &&
                (c.Name.ToLower().Contains(term) || c.Email.ToLower().Contains(term) ||
                 c.Phone.Contains(term) || (c.Nid != null && c.Nid.Contains(term)) ||
                 (c.CompanyName != null && c.CompanyName.ToLower().Contains(term)) ||
                 (c.ClientCode != null && c.ClientCode.Contains(term))))
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
        return clients.Select(MapToDto);
    }

    public async Task<int> GetCountAsync(Guid chamberId, string? search = null, string? status = null, string? clientType = null)
    {
        return await BuildQuery(chamberId, search, status, clientType).CountAsync();
    }

    public async Task<(bool Success, string Message, ClientResponseDto? Data)> UploadAvatarAsync(Guid clientId, string avatarUrl)
    {
        var client = await _context.Clients.FindAsync(clientId);
        if (client == null || client.IsDeleted)
            return (false, "Client not found", null);

        client.AvatarUrl = avatarUrl;
        client.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (true, "Avatar uploaded", MapToDto(client));
    }

    private async Task<string> GenerateClientCode(Guid chamberId)
    {
        var count = await _context.Clients.CountAsync(c => c.ChamberId == chamberId) + 1;
        return $"CL-{DateTime.UtcNow:yyyy}-{count:D4}";
    }

    private IQueryable<Client> BuildQuery(Guid chamberId, string? search, string? status, string? clientType)
    {
        var query = _context.Clients
            .Include(c => c.ClientCases.Where(cc => !cc.IsDeleted))
            .Where(c => c.ChamberId == chamberId && !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var active = status.Equals("active", StringComparison.OrdinalIgnoreCase);
            query = query.Where(c => c.IsActive == active);
        }

        if (!string.IsNullOrWhiteSpace(clientType))
        {
            var type = clientType.ToLower();
            query = query.Where(c => c.ClientType != null && c.ClientType.ToLower() == type);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) || c.Email.ToLower().Contains(term) ||
                c.Phone.Contains(term) || (c.Nid != null && c.Nid.Contains(term)) ||
                (c.CompanyName != null && c.CompanyName.ToLower().Contains(term)) ||
                (c.ClientCode != null && c.ClientCode.Contains(term)));
        }

        return query;
    }

    private static ClientResponseDto MapToDto(Client c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Phone = c.Phone,
        Email = c.Email,
        Address = c.Address,
        Nid = c.Nid,
        CompanyName = c.CompanyName,
        Notes = c.Notes,
        IsActive = c.IsActive,
        AvatarUrl = c.AvatarUrl,
        CasesCount = c.ClientCases.Count,
        CreatedAt = c.CreatedAt,
        ClientType = c.ClientType,
        ClientCode = c.ClientCode,
        PassportNumber = c.PassportNumber,
        DateOfBirth = c.DateOfBirth,
        Gender = c.Gender,
        Occupation = c.Occupation,
        Nationality = c.Nationality,
        TradeLicense = c.TradeLicense,
        RegistrationNumber = c.RegistrationNumber,
        TaxVatNumber = c.TaxVatNumber,
        AuthorizedRepresentative = c.AuthorizedRepresentative,
        Tags = c.Tags,
        RiskLevel = c.RiskLevel,
        ClientCategory = c.ClientCategory,
        BillingPreference = c.BillingPreference,
        PaymentTerms = c.PaymentTerms,
        CreditLimit = c.CreditLimit,
        PreferredContactMethod = c.PreferredContactMethod,
        WhatsAppNumber = c.WhatsAppNumber,
        SecondaryPhone = c.SecondaryPhone,
        EmergencyContact = c.EmergencyContact,
        IsBlacklisted = c.IsBlacklisted,
    };
}
