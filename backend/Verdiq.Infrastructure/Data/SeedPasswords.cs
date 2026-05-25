namespace Verdiq.Infrastructure.Data;

/// <summary>
/// Pre-computed BCrypt hashes for seed users. HasData must not call HashPassword at model build time
/// because each build produces a different salt and breaks deterministic seeding.
/// </summary>
public static class SeedPasswords
{
    public const string Admin = "$2a$11$VyGwoqxHC6gMQ9iMsda/7eE9a5TV9SOHBRyX4SgwU.RJNNxnYEera";
    public const string Lawyer = "$2a$11$CnI9Ur82n8LPzJkcFCD6Q.D4J892KK5RHTh7BAXnHCmKE3cQOxOey";
}
