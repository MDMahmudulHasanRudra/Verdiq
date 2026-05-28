namespace Verdiq.Application.DTOs.ChamberSettings;

public class ChamberSettingsDto
{
    public Guid Id { get; set; }
    public Guid ChamberId { get; set; }
    public Dictionary<string, object> Settings { get; set; } = new();
    public DateTime UpdatedAt { get; set; }
}

public class UpdateChamberSettingsDto
{
    public Dictionary<string, object>? General { get; set; }
    public Dictionary<string, object>? CaseDefaults { get; set; }
    public Dictionary<string, object>? ClientManagement { get; set; }
    public Dictionary<string, object>? Billing { get; set; }
    public Dictionary<string, object>? DocumentManagement { get; set; }
    public Dictionary<string, object>? HearingsReminders { get; set; }
    public Dictionary<string, object>? LegalDrafting { get; set; }
    public Dictionary<string, object>? Notifications { get; set; }
    public Dictionary<string, object>? AiAssistant { get; set; }
    public Dictionary<string, object>? SecuritySession { get; set; }
    public Dictionary<string, object>? DashboardUi { get; set; }
}
