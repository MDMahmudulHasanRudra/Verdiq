using Microsoft.EntityFrameworkCore;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;

namespace Verdiq.Infrastructure.Data;

public static class DatabaseInitializer
{
    private static readonly Guid DefaultChamberId = Guid.Parse("c0000000-0000-0000-0000-000000000001");
    private static readonly Guid AdminId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
    private static readonly Guid LawyerId = Guid.Parse("e5f6a7b8-c9d0-1234-5678-9abcdef01234");
    private static readonly Guid SubId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");

    public static async Task InitializeAsync(AppDbContext db)
    {
        if (!await db.Database.CanConnectAsync())
            throw new InvalidOperationException("Cannot connect to the database.");

        if (!await TableExistsAsync(db, "Users"))
            await db.Database.EnsureCreatedAsync();

        await ApplySchemaUpdatesAsync(db);
        await EnsureDefaultDataAsync(db);
    }

    private static async Task<bool> TableExistsAsync(AppDbContext db, string tableName)
    {
        var connection = db.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = @name)";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "name";
        parameter.Value = tableName;
        command.Parameters.Add(parameter);

        var result = await command.ExecuteScalarAsync();
        return result is true or 1 or 1L;
    }

    private static async Task ApplySchemaUpdatesAsync(AppDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
            ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "TwoFactorEnabled" boolean NOT NULL DEFAULT false;
            ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "TwoFactorSecret" character varying(256);
            ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "TwoFactorVerifiedAt" timestamp with time zone;
            ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "LoginAttempts" integer NOT NULL DEFAULT 0;
            ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "LockoutEnd" timestamp with time zone;
            ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "LastLoginAt" timestamp with time zone;
            ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "LastLoginIp" character varying(50);
            """);

        await db.Database.ExecuteSqlRawAsync("""
            ALTER TABLE "Documents" ADD COLUMN IF NOT EXISTS "StorageProvider" character varying(50) NOT NULL DEFAULT 'Local';
            ALTER TABLE "Documents" ADD COLUMN IF NOT EXISTS "StorageKey" character varying(500);
            ALTER TABLE "Documents" ADD COLUMN IF NOT EXISTS "Version" integer NOT NULL DEFAULT 1;
            """);
    }

    public static async Task EnsureDefaultDataAsync(AppDbContext db)
    {
        if (!await db.Chambers.IgnoreQueryFilters().AnyAsync(c => c.Id == DefaultChamberId))
        {
            db.Chambers.Add(new Chamber
            {
                Id = DefaultChamberId,
                Name = "Verdiq Chamber",
                Address = "42 Gulshan Avenue, Dhaka",
                Phone = "+8801700000000",
                SubscriptionPlan = SubscriptionPlan.Chamber,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }

        var seedUsers = new[]
        {
            new
            {
                Id = AdminId,
                FullName = "Admin Verdiq",
                Email = "admin@verdiq.com",
                PasswordHash = SeedPasswords.Admin,
                Phone = "+8801700000000",
                Role = UserRole.Owner,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Id = LawyerId,
                FullName = "Adv. Abdul Karim",
                Email = "lawyer@verdiq.com",
                PasswordHash = SeedPasswords.Lawyer,
                Phone = "+8801712345678",
                Role = UserRole.SeniorLawyer,
                CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            }
        };

        foreach (var seed in seedUsers)
        {
            var user = await db.Users.IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == seed.Email);

            if (user == null)
            {
                user = new User
                {
                    Id = seed.Id,
                    FullName = seed.FullName,
                    Email = seed.Email,
                    PasswordHash = seed.PasswordHash,
                    Phone = seed.Phone,
                    Role = seed.Role,
                    IsActive = true,
                    ChamberId = DefaultChamberId,
                    CreatedAt = seed.CreatedAt
                };
                db.Users.Add(user);
            }
            else
            {
                user.ChamberId = DefaultChamberId;
                user.IsActive = true;
                user.IsDeleted = false;
                user.LoginAttempts = 0;
                user.LockoutEnd = null;
            }
        }

        if (!await db.Subscriptions.IgnoreQueryFilters().AnyAsync(s => s.Id == SubId))
        {
            db.Subscriptions.Add(new Subscription
            {
                Id = SubId,
                ChamberId = DefaultChamberId,
                Plan = SubscriptionPlan.Chamber,
                Status = SubscriptionStatus.Active,
                CurrentPeriodStart = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CurrentPeriodEnd = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }

        await db.SaveChangesAsync();
    }
}
