using System.Text.RegularExpressions;
using Verdiq.Application.DTOs.Auth;

namespace Verdiq.Application.Validators;

public static class AuthValidators
{
    public static (bool IsValid, string Error) ValidateRegister(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName))
            return (false, "Full name is required");

        if (string.IsNullOrWhiteSpace(dto.Email) || !Regex.IsMatch(dto.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return (false, "Valid email is required");

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
            return (false, "Password must be at least 6 characters");

        if (dto.Password != dto.ConfirmPassword)
            return (false, "Passwords do not match");

        if (string.IsNullOrWhiteSpace(dto.Phone))
            return (false, "Phone number is required");

        var validRoles = new[] { "admin", "lawyer", "assistant", "client" };
        if (!validRoles.Contains(dto.Role.ToLower()))
            return (false, "Invalid role. Must be: admin, lawyer, assistant, or client");

        return (true, string.Empty);
    }

    public static (bool IsValid, string Error) ValidateLogin(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return (false, "Email is required");

        if (string.IsNullOrWhiteSpace(dto.Password))
            return (false, "Password is required");

        return (true, string.Empty);
    }
}
