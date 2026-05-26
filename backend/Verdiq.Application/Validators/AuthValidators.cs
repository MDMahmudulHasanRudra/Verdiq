namespace Verdiq.Application.Validators;

public static class AuthValidators
{
    public static (bool Valid, string Error) ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return (false, "Email is required");
        if (!email.Contains('@') || !email.Contains('.'))
            return (false, "Invalid email format");
        return (true, string.Empty);
    }

    public static (bool Valid, string Error) ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            return (false, "Password must be at least 6 characters");
        return (true, string.Empty);
    }

    public static (bool Valid, string Error) ValidatePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return (false, "Phone is required");
        return (true, string.Empty);
    }
}
