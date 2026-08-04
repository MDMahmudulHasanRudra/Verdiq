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

        if (dto.General != null) current["general"] = MergeSection(GetSection(current, "general"), dto.General);
        if (dto.Branding != null) current["branding"] = MergeSection(GetSection(current, "branding"), dto.Branding);
        if (dto.CaseDefaults != null) current["caseDefaults"] = MergeSection(GetSection(current, "caseDefaults"), dto.CaseDefaults);
        if (dto.ClientManagement != null) current["clientManagement"] = MergeSection(GetSection(current, "clientManagement"), dto.ClientManagement);
        if (dto.Billing != null) current["billing"] = MergeSection(GetSection(current, "billing"), dto.Billing);
        if (dto.DocumentManagement != null) current["documentManagement"] = MergeSection(GetSection(current, "documentManagement"), dto.DocumentManagement);
        if (dto.HearingsReminders != null) current["hearingsReminders"] = MergeSection(GetSection(current, "hearingsReminders"), dto.HearingsReminders);
        if (dto.LegalDrafting != null) current["legalDrafting"] = MergeSection(GetSection(current, "legalDrafting"), dto.LegalDrafting);
        if (dto.Communications != null) current["communications"] = MergeSection(GetSection(current, "communications"), dto.Communications);
        if (dto.Notifications != null) current["notifications"] = MergeSection(GetSection(current, "notifications"), dto.Notifications);
        if (dto.AiAssistant != null) current["aiAssistant"] = MergeSection(GetSection(current, "aiAssistant"), dto.AiAssistant);
        if (dto.SecuritySession != null) current["securitySession"] = MergeSection(GetSection(current, "securitySession"), dto.SecuritySession);
        if (dto.DashboardUi != null) current["dashboardUi"] = MergeSection(GetSection(current, "dashboardUi"), dto.DashboardUi);
        if (dto.Integrations != null) current["integrations"] = MergeSection(GetSection(current, "integrations"), dto.Integrations);
        if (dto.DataRetention != null) current["dataRetention"] = MergeSection(GetSection(current, "dataRetention"), dto.DataRetention);
        if (dto.Workflow != null) current["workflow"] = MergeSection(GetSection(current, "workflow"), dto.Workflow);

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
                ["website"] = "",
                ["timezone"] = "Asia/Dhaka",
                ["dateFormat"] = "DD-MM-YYYY",
                ["currency"] = "BDT",
                ["language"] = "en",
                ["fiscalYearStart"] = "July",
            },
            ["branding"] = new Dictionary<string, object>
            {
                ["themeColor"] = "#0f766e",
                ["accentColor"] = "#f59e0b",
                ["showBranding"] = true,
                ["appName"] = "Verdiq",
                ["appNameBn"] = "ভার্দিক",
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
            ["workflow"] = new Dictionary<string, object>
            {
                ["autoCaseNumbering"] = true,
                ["defaultCaseStatus"] = "Active",
                ["requireWorkflowNotes"] = false,
                ["allowCaseReopen"] = true,
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
                ["invoiceDueDays"] = 14,
                ["lateFeePercent"] = 2,
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
            ["communications"] = new Dictionary<string, object>
            {
                ["defaultReminderChannel"] = "email",
                ["allowSms"] = false,
                ["allowWhatsApp"] = false,
                ["allowEmail"] = true,
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
            ["integrations"] = new Dictionary<string, object>
            {
                ["googleDriveEnabled"] = false,
                ["dropboxEnabled"] = false,
                ["storageProvider"] = "local",
                ["emailProvider"] = "smtp",
            },
            ["dataRetention"] = new Dictionary<string, object>
            {
                ["archiveAfterDays"] = 365,
                ["autoDeleteAfterDays"] = 0,
                ["retainAuditLogsDays"] = 2555,
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

    private static Dictionary<string, object> MergeSection(Dictionary<string, object>? existing, Dictionary<string, object>? incoming)
    {
        var merged = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        if (existing != null)
        {
            foreach (var item in existing)
            {
                merged[item.Key] = item.Value;
            }
        }

        if (incoming != null)
        {
            foreach (var item in incoming)
            {
                merged[item.Key] = item.Value;
            }
        }

        return merged;
    }

    private static Dictionary<string, object> GetSection(Dictionary<string, object> current, string key)
    {
        if (current.TryGetValue(key, out var value) && value is Dictionary<string, object> section)
        {
            return section;
        }

        return new Dictionary<string, object>();
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
