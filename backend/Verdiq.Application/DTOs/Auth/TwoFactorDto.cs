namespace Verdiq.Application.DTOs.Auth;

public class TwoFactorSetupResponse
{
    public string Secret { get; set; } = string.Empty;
    public string QrCodeUrl { get; set; } = string.Empty;
    public string ManualEntryKey { get; set; } = string.Empty;
}

public class TwoFactorVerifyDto
{
    public string Code { get; set; } = string.Empty;
}

public class TwoFactorLoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? TempToken { get; set; }
}
