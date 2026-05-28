using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Verdiq.Application.DTOs.ChamberSettings;
using Verdiq.Application.Interfaces;
using Verdiq.Domain.Entities;
using Verdiq.Infrastructure.Data;

namespace Verdiq.Infrastructure.Services;

public class ChamberSettingsService : IChamberSettingsService
{
    private readonly AppDbContext _context;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    public ChamberSettingsService(AppDbContext context) => _context = context;

    public async Task<(bool Success, string Message, ChamberSettingsDto? Data)> GetSettingsAsync(Guid chamberId)
    {
        var settings = await _context.ChamberSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ChamberId == chamberId);

        if (settings == null)
        {
            var defaults = CreateDefaultSettings();
            settings = new ChamberSettings
            {
                ChamberId = chamberId,
                SettingsJson = JsonSerializer.Serialize(defaults, JsonOptions),
            };
            _context.ChamberSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        var dto = MapToDto(settings);
        return (true, "Settings retrieved", dto);
    }

    public async Task<(bool Success, string Message, ChamberSettingsDto? Data)> UpdateSettingsAsync(Guid chamberId, UpdateChamberSettingsDto dto, Guid userId)
    {
        var settings = await _context.ChamberSettings
            .FirstOrDefaultAsync(s => s.ChamberId == chamberId);

        var current = settings != null
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(settings.SettingsJson, JsonOptions) ?? new()
            : CreateDefaultSettings();

        if (dto.General != null) current["general"] = dto.General;
        if (dto.CaseDefaults != null) current["caseDefaults"] = dto.CaseDefaults;
        if (dto.ClientManagement != null) current["clientManagement"] = dto.ClientManagement;
        if (dto.Billing != null) current["billing"] = dto.Billing;
        if (dto.DocumentManagement != null) current["documentManagement"] = dto.DocumentManagement;
        if (dto.HearingsReminders != null) current["hearingsReminders"] = dto.HearingsReminders;
        if (dto.LegalDrafting != null) current["legalDrafting"] = dto.LegalDrafting;
        if (dto.Notifications != null) current["notifications"] = dto.Notifications;
        if (dto.AiAssistant != null) current["aiAssistant"] = dto.AiAssistant;
        if (dto.SecuritySession != null) current["securitySession"] = dto.SecuritySession;
        if (dto.DashboardUi != null) current["dashboardUi"] = dto.DashboardUi;

        if (settings == null)
        {
            settings = new ChamberSettings { ChamberId = chamberId };
            _context.ChamberSettings.Add(settings);
        }

        settings.SettingsJson = JsonSerializer.Serialize(current, JsonOptions);
        settings.UpdatedBy = userId;
        settings.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var result = MapToDto(settings);
        return (true, "Settings updated", result);
    }

    public async Task<(bool Success, string Message, object? Data)> GetSubsectionAsync(Guid chamberId, string subsection)
    {
        var settings = await _context.ChamberSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ChamberId == chamberId);

        if (settings == null)
            return (false, "Settings not found. Create settings first.", null);

        var all = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(settings.SettingsJson, JsonOptions);
        if (all == null || !all.TryGetValue(subsection, out var value))
            return (false, $"Subsection '{subsection}' not found", null);

        var parsed = JsonSerializer.Deserialize<object>(value.GetRawText(), JsonOptions);
        return (true, "Subsection retrieved", parsed);
    }

    public async Task<(bool Success, string Message, ChamberSettingsDto? Data)> UpdateSubsectionAsync(Guid chamberId, string subsection, Dictionary<string, object> values, Guid userId)
    {
        var settings = await _context.ChamberSettings
            .FirstOrDefaultAsync(s => s.ChamberId == chamberId);

        if (settings == null)
        {
            var defaults = CreateDefaultSettings();
            defaults[subsection] = values;
            settings = new ChamberSettings
            {
                ChamberId = chamberId,
                SettingsJson = JsonSerializer.Serialize(defaults, JsonOptions),
                UpdatedBy = userId,
                UpdatedAt = DateTime.UtcNow,
            };
            _context.ChamberSettings.Add(settings);
            await _context.SaveChangesAsync();

            var result = MapToDto(settings);
            return (true, "Settings created with subsection", result);
        }

        var current = JsonSerializer.Deserialize<Dictionary<string, object>>(settings.SettingsJson, JsonOptions) ?? new();
        current[subsection] = values;
        settings.SettingsJson = JsonSerializer.Serialize(current, JsonOptions);
        settings.UpdatedBy = userId;
        settings.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var dto = MapToDto(settings);
        return (true, "Subsection updated", dto);
    }

    private static Dictionary<string, object> CreateDefaultSettings()
    {
        return new Dictionary<string, object>
        {
            ["general"] = new Dictionary<string, object>
            {
                ["companyName"] = "Verdiq Law Chamber",
                ["companyNameBn"] = "",
                ["logoUrl"] = "",
                ["address"] = "",
                ["phone"] = "",
                ["email"] = "",
                ["timezone"] = "Asia/Dhaka",
                ["dateFormat"] = "DD-MM-YYYY",
                ["currency"] = "BDT",
                ["language"] = "en",
            },
            ["caseDefaults"] = new Dictionary<string, object>
            {
                ["caseNumberPrefix"] = "VER",
                ["caseNumberFormat"] = "{PREFIX}-{YYYY}-{XXXX}",
                ["caseTypes"] = new[] { "Criminal", "Civil", "Family", "Corporate", "Tax", "Labor", "Property" },
                ["priorityLevels"] = new[] { "Low", "Medium", "High", "Urgent" },
                ["statuses"] = new[] { "Active", "Pending", "Closed", "Appeal", "Withdrawn" },
                ["courtPresets"] = new[] { "Dhaka District Court", "High Court Division", "Supreme Court" },
            },
            ["clientManagement"] = new Dictionary<string, object>
            {
                ["clientTypes"] = new[] { "Individual", "Company", "NGO", "Government" },
                ["enablePortalAccess"] = true,
                ["portalRegistrationApproval"] = false,
                ["defaultDocumentSharing"] = false,
            },
            ["billing"] = new Dictionary<string, object>
            {
                ["taxRatePercent"] = 15,
                ["invoicePrefix"] = "INV",
                ["paymentMethods"] = new[] { "Bkash", "Nagad", "Card", "Bank Transfer", "Cash" },
                ["expenseCategories"] = new[] { "Court Fees", "Stamp Fees", "Transport", "Stationery", "Admin", "Other" },
            },
            ["documentManagement"] = new Dictionary<string, object>
            {
                ["categories"] = new[] { "Pleading", "Evidence", "Correspondence", "Court Order", "Contract", "Other" },
                ["maxFileSizeMb"] = 25,
                ["allowedMimeTypes"] = new[] { "application/pdf", "image/jpeg", "image/png", "application/msword" },
                ["enableOcr"] = false,
                ["storageProvider"] = "local",
            },
            ["hearingsReminders"] = new Dictionary<string, object>
            {
                ["hearingTypes"] = new[] { "Appearance", "Argument", "Order", "Judgment" },
                ["reminderOffsetsDays"] = new[] { 1, 3, 7 },
                ["enableEmailReminders"] = true,
                ["enableSmsReminders"] = false,
                ["enableWhatsAppReminders"] = false,
                ["defaultReminderChannel"] = "email",
            },
            ["legalDrafting"] = new Dictionary<string, object>
            {
                ["templateCategories"] = new[] { "Petition", "Affidavit", "Contract", "Notice", "Deed" },
                ["enableSmartVariables"] = true,
            },
            ["notifications"] = new Dictionary<string, object>
            {
                ["enableEmailNotifications"] = true,
                ["enablePushNotifications"] = true,
                ["smtpConfigured"] = false,
                ["smsConfigured"] = false,
                ["whatsappConfigured"] = false,
            },
            ["aiAssistant"] = new Dictionary<string, object>
            {
                ["enabled"] = true,
                ["apiKeyConfigured"] = false,
                ["model"] = "gpt-4o-mini",
            },
            ["securitySession"] = new Dictionary<string, object>
            {
                ["enableMfa"] = false,
                ["sessionTimeoutMinutes"] = 60,
                ["maxLoginAttempts"] = 5,
                ["lockoutDurationMinutes"] = 15,
            },
            ["dashboardUi"] = new Dictionary<string, object>
            {
                ["companyName"] = "Verdiq Law Chamber",
                ["showWelcomeWidget"] = true,
                ["showCaseStats"] = true,
                ["showHearingWidget"] = true,
                ["defaultWidgets"] = new[] { "caseStats", "upcomingHearings", "recentActivities", "invoiceSummary" },
            },
        };
    }

    private static ChamberSettingsDto MapToDto(ChamberSettings s)
    {
        var parsed = JsonSerializer.Deserialize<Dictionary<string, object>>(s.SettingsJson, JsonOptions) ?? new();
        return new ChamberSettingsDto
        {
            Id = s.Id,
            ChamberId = s.ChamberId,
            Settings = parsed,
            UpdatedAt = s.UpdatedAt ?? s.CreatedAt,
        };
    }
}
