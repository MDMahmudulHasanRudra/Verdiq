namespace Verdiq.Application.DTOs.Auth;

public class RegisterDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Role { get; set; } = "Lawyer";
    public string? BarCouncilId { get; set; }
    public Guid ChamberId { get; set; }
}