using Microsoft.EntityFrameworkCore;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;

namespace Verdiq.Infrastructure.Data;

public static class DatabaseInitializer
{
    private static readonly Guid DefaultOrgId = Guid.Parse("a0000000-0000-0000-0000-000000000001");
    private static readonly Guid AdminId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
    private static readonly Guid AdminSubId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");
    private static readonly Guid LawyerId = Guid.Parse("e5f6a7b8-c9d0-1234-5678-9abcdef01234");
    private static readonly Guid LawyerSubId = Guid.Parse("f6a7b8c9-d0e1-2345-6789-abcdef012345");
    private static readonly Guid OrgMemberId = Guid.Parse("a0000000-0000-0000-0000-000000000002");

    public static async Task InitializeAsync(AppDbContext db)
    {
        if (!await db.Database.CanConnectAsync())
            throw new InvalidOperationException("Cannot connect to the database.");

        if (!await TableExistsAsync(db, "Users"))
            await db.Database.EnsureCreatedAsync();

        await ApplySchemaUpdatesAsync(db);
        await EnsureDefaultUsersAsync(db);
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
            CREATE TABLE IF NOT EXISTS "Organizations" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "Name" character varying(255) NOT NULL,
                "Slug" character varying(100),
                "Description" character varying(2000),
                "LogoUrl" character varying(500),
                "Website" character varying(500),
                "Address" character varying(500),
                "Phone" character varying(20),
                "Email" character varying(255),
                "IsActive" boolean NOT NULL DEFAULT true,
                "OwnerId" uuid NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone,
                "IsDeleted" boolean NOT NULL DEFAULT false
            );

            CREATE TABLE IF NOT EXISTS "OrganizationMembers" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "OrganizationId" uuid NOT NULL REFERENCES "Organizations"("Id") ON DELETE CASCADE,
                "UserId" uuid REFERENCES "Users"("Id") ON DELETE CASCADE,
                "InvitedEmail" character varying(255),
                "Role" character varying(20) NOT NULL,
                "InvitedAt" timestamp with time zone,
                "AcceptedAt" timestamp with time zone,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone,
                "IsDeleted" boolean NOT NULL DEFAULT false
            );

            CREATE UNIQUE INDEX IF NOT EXISTS "IX_OrganizationMembers_OrganizationId_UserId"
                ON "OrganizationMembers" ("OrganizationId", "UserId");

            CREATE TABLE IF NOT EXISTS "Workspaces" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "OrganizationId" uuid NOT NULL REFERENCES "Organizations"("Id") ON DELETE CASCADE,
                "Name" character varying(255) NOT NULL,
                "Description" character varying(1000),
                "Color" character varying(20),
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone,
                "IsDeleted" boolean NOT NULL DEFAULT false
            );

            CREATE TABLE IF NOT EXISTS "DocumentVersions" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "DocumentId" uuid NOT NULL REFERENCES "Documents"("Id") ON DELETE CASCADE,
                "VersionNumber" integer NOT NULL,
                "FileName" character varying(500) NOT NULL,
                "OriginalFileName" character varying(500) NOT NULL,
                "ContentType" character varying(100) NOT NULL,
                "FileSize" bigint NOT NULL,
                "FilePath" text NOT NULL,
                "StorageProvider" character varying(50) NOT NULL DEFAULT 'Local',
                "StorageKey" character varying(500),
                "ChangeNotes" character varying(2000),
                "Status" character varying(20) NOT NULL,
                "UploadedById" uuid NOT NULL REFERENCES "Users"("Id"),
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone,
                "IsDeleted" boolean NOT NULL DEFAULT false
            );

            CREATE TABLE IF NOT EXISTS "DocumentTags" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "DocumentId" uuid NOT NULL REFERENCES "Documents"("Id") ON DELETE CASCADE,
                "TagName" character varying(100) NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone,
                "IsDeleted" boolean NOT NULL DEFAULT false
            );

            CREATE UNIQUE INDEX IF NOT EXISTS "IX_DocumentTags_DocumentId_TagName"
                ON "DocumentTags" ("DocumentId", "TagName");

            CREATE TABLE IF NOT EXISTS "AiConversations" (
                "Id" uuid NOT NULL PRIMARY KEY,
                "UserId" uuid NOT NULL REFERENCES "Users"("Id") ON DELETE CASCADE,
                "Role" character varying(20) NOT NULL,
                "Content" character varying(10000) NOT NULL,
                "TokensUsed" integer NOT NULL DEFAULT 0,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone,
                "IsDeleted" boolean NOT NULL DEFAULT false
            );
            """);

        await db.Database.ExecuteSqlRawAsync("""
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "OrganizationId" uuid;
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "OrganizationId" uuid;
            ALTER TABLE "Hearings" ADD COLUMN IF NOT EXISTS "OrganizationId" uuid;
            ALTER TABLE "Documents" ADD COLUMN IF NOT EXISTS "OrganizationId" uuid;
            ALTER TABLE "Documents" ADD COLUMN IF NOT EXISTS "StorageProvider" character varying(50) NOT NULL DEFAULT 'Local';
            ALTER TABLE "Documents" ADD COLUMN IF NOT EXISTS "StorageKey" character varying(500);
            ALTER TABLE "Documents" ADD COLUMN IF NOT EXISTS "CurrentVersion" integer NOT NULL DEFAULT 1;
            ALTER TABLE "Payments" ADD COLUMN IF NOT EXISTS "Description" character varying(2000);
            ALTER TABLE "Payments" ADD COLUMN IF NOT EXISTS "FailureReason" text;
            """);

        await db.Database.ExecuteSqlRawAsync($"""
            UPDATE "Cases" SET "OrganizationId" = '{DefaultOrgId}' WHERE "OrganizationId" IS NULL;
            UPDATE "Clients" SET "OrganizationId" = '{DefaultOrgId}' WHERE "OrganizationId" IS NULL;
            UPDATE "Hearings" SET "OrganizationId" = '{DefaultOrgId}' WHERE "OrganizationId" IS NULL;
            UPDATE "Documents" SET "OrganizationId" = '{DefaultOrgId}' WHERE "OrganizationId" IS NULL;
            """);
    }

    public static async Task EnsureDefaultUsersAsync(AppDbContext db)
    {
        var seedUsers = new[]
        {
            new
            {
                Id = AdminId,
                FullName = "Admin Verdiq",
                Email = "admin@verdiq.com",
                PasswordHash = SeedPasswords.Admin,
                Phone = "+8801700000000",
                Role = UserRole.Admin,
                SubId = AdminSubId,
                Plan = SubscriptionPlan.Chamber,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new
            {
                Id = LawyerId,
                FullName = "Adv. Abdul Karim",
                Email = "lawyer@verdiq.com",
                PasswordHash = SeedPasswords.Lawyer,
                Phone = "+8801712345678",
                Role = UserRole.Lawyer,
                SubId = LawyerSubId,
                Plan = SubscriptionPlan.Pro,
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
                    CreatedAt = seed.CreatedAt
                };
                db.Users.Add(user);
            }
            else
            {
                user.IsActive = true;
                user.IsDeleted = false;
                user.LoginAttempts = 0;
                user.LockoutEnd = null;
            }

            var subscription = await db.Subscriptions.IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.UserId == seed.Id);

            if (subscription == null)
            {
                db.Subscriptions.Add(new Subscription
                {
                    Id = seed.SubId,
                    UserId = seed.Id,
                    Plan = seed.Plan,
                    Status = SubscriptionStatus.Active,
                    CurrentPeriodStart = seed.CreatedAt,
                    CurrentPeriodEnd = seed.CreatedAt.AddYears(1),
                    CreatedAt = seed.CreatedAt
                });
            }
        }

        if (!await db.Organizations.IgnoreQueryFilters().AnyAsync(o => o.Id == DefaultOrgId))
        {
            db.Organizations.Add(new Organization
            {
                Id = DefaultOrgId,
                Name = "Verdiq Chamber",
                Slug = "verdiq-chamber",
                Description = "Default organization for Verdiq legal practice",
                IsActive = true,
                OwnerId = LawyerId,
                CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            });
        }

        if (!await db.OrganizationMembers.IgnoreQueryFilters().AnyAsync(m => m.Id == OrgMemberId))
        {
            db.OrganizationMembers.Add(new OrganizationMember
            {
                Id = OrgMemberId,
                OrganizationId = DefaultOrgId,
                UserId = LawyerId,
                Role = OrganizationRole.Owner,
                AcceptedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            });
        }

        await db.SaveChangesAsync();
    }
}
