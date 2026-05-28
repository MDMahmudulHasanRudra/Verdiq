using Microsoft.EntityFrameworkCore;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Task = System.Threading.Tasks.Task;

namespace Verdiq.Infrastructure.Data;

public static class DatabaseInitializer
{
    private static readonly Guid DefaultChamberId = Guid.Parse("c0000000-0000-0000-0000-000000000001");
    private static readonly Guid AdminId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
    private static readonly Guid LawyerId = Guid.Parse("e5f6a7b8-c9d0-1234-5678-9abcdef01234");
    private static readonly Guid SubId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");
    private static readonly Guid DefaultOrganizationId = Guid.Parse("d0000000-0000-0000-0000-000000000001");

    public static async Task InitializeAsync(AppDbContext db)
    {
        if (!await db.Database.CanConnectAsync())
            throw new InvalidOperationException("Cannot connect to the database.");

        // EnsureCreatedAsync creates all tables if the database has no tables from the model.
        // It's a no-op if any model table already exists (a known limitation).
        await db.Database.EnsureCreatedAsync();

        // If a key table from the current model is still missing, the database was created
        // with an older schema. Drop and recreate to get the full latest schema.
        if (!await TableExistsAsync(db, "Chambers"))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
        }

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
        // Users table — Phase 7 additions
        await db.Database.ExecuteSqlRawAsync("""
            ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "TwoFactorEnabled" boolean NOT NULL DEFAULT false;
            ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "TwoFactorSecret" character varying(256);
            ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "TwoFactorVerifiedAt" timestamp with time zone;
            ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "LoginAttempts" integer NOT NULL DEFAULT 0;
            ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "LockoutEnd" timestamp with time zone;
            ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "LastLoginAt" timestamp with time zone;
            ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "LastLoginIp" character varying(50);
            ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "ClientId" uuid;
            """);

        // Documents table — Phase 5/7 additions
        await db.Database.ExecuteSqlRawAsync("""
            ALTER TABLE "Documents" ADD COLUMN IF NOT EXISTS "StorageProvider" character varying(50) NOT NULL DEFAULT 'Local';
            ALTER TABLE "Documents" ADD COLUMN IF NOT EXISTS "StorageKey" character varying(500);
            ALTER TABLE "Documents" ADD COLUMN IF NOT EXISTS "Version" integer NOT NULL DEFAULT 1;
            ALTER TABLE "Documents" ADD COLUMN IF NOT EXISTS "Visibility" character varying(50) NOT NULL DEFAULT 'InternalOnly';
            ALTER TABLE "Documents" ADD COLUMN IF NOT EXISTS "SharedWithClientId" uuid;
            """);

        // Clients table — Phase 7 columns
        await db.Database.ExecuteSqlRawAsync("""
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "ClientType" character varying(100);
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "ClientCode" character varying(100);
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "PassportNumber" character varying(50);
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "DateOfBirth" timestamp with time zone;
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "Gender" character varying(20);
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "Occupation" character varying(200);
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "Nationality" character varying(100);
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "TradeLicense" character varying(200);
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "RegistrationNumber" character varying(200);
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "TaxVatNumber" character varying(200);
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "AuthorizedRepresentative" character varying(500);
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "Tags" character varying(1000);
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "RiskLevel" character varying(20);
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "ClientCategory" character varying(100);
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "BillingPreference" character varying(100);
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "PaymentTerms" character varying(100);
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "CreditLimit" numeric;
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "PreferredContactMethod" character varying(50);
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "WhatsAppNumber" character varying(50);
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "SecondaryPhone" character varying(50);
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "EmergencyContact" character varying(500);
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "IsBlacklisted" boolean NOT NULL DEFAULT false;
            ALTER TABLE "Clients" ADD COLUMN IF NOT EXISTS "UserId" uuid;
            """);

        // Cases table — Phase 7 columns
        await db.Database.ExecuteSqlRawAsync("""
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "ActsAndSections" character varying(1000);
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "FirNumber" character varying(100);
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "PoliceStation" character varying(200);
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "ClosingDate" timestamp with time zone;
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "GdNumber" character varying(100);
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "JudgeName" character varying(200);
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "Bench" character varying(200);
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "Prosecutor" character varying(200);
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "OpposingLawyer" character varying(200);
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "Jurisdiction" character varying(200);
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "AppealStatus" character varying(100);
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "RiskLevel" character varying(20);
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "ComplexityScore" integer;
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "PracticeArea" character varying(200);
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "Department" character varying(200);
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "InternalNotes" character varying(4000);
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "RetainerAmount" numeric;
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "BillingMethod" character varying(50);
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "FixedFee" numeric;
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "HourlyRate" numeric;
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "BudgetLimit" numeric;
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "ExpenseBudget" numeric;
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "NextHearingDate" timestamp with time zone;
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "CriticalDeadlines" character varying(2000);
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "LimitationExpiry" timestamp with time zone;
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "CaseTemplateId" uuid;
            ALTER TABLE "Cases" ADD COLUMN IF NOT EXISTS "WorkflowTemplateId" uuid;
            """);

        // ClientCases join table — add Role column
        await db.Database.ExecuteSqlRawAsync("""
            ALTER TABLE "ClientCases" ADD COLUMN IF NOT EXISTS "Role" character varying(100);
            """);

        // CaseActivities — Phase 5 columns (added later)
        await db.Database.ExecuteSqlRawAsync("""
            ALTER TABLE "CaseActivities" ADD COLUMN IF NOT EXISTS "IsClientVisible" boolean NOT NULL DEFAULT true;
            """);

        // New Phase 7 tables
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "ChamberSettings" (
                "Id" uuid NOT NULL,
                "ChamberId" uuid NOT NULL,
                "SettingsJson" jsonb NOT NULL,
                "UpdatedBy" uuid,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone,
                "IsDeleted" boolean NOT NULL DEFAULT false,
                CONSTRAINT "PK_ChamberSettings" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_ChamberSettings_Chambers_ChamberId" FOREIGN KEY ("ChamberId") REFERENCES "Chambers"("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_ChamberSettings_Users_UpdatedBy" FOREIGN KEY ("UpdatedBy") REFERENCES "Users"("Id") ON DELETE SET NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_ChamberSettings_ChamberId" ON "ChamberSettings"("ChamberId");
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "LegalSections" (
                "Id" uuid NOT NULL,
                "SectionCode" character varying(50) NOT NULL,
                "SectionTitle" character varying(500) NOT NULL,
                "LawName" character varying(255) NOT NULL,
                "Country" character varying(100),
                "Category" character varying(100),
                "Description" character varying(4000),
                "Severity" character varying(20),
                "IsActive" boolean NOT NULL DEFAULT true,
                "ChamberId" uuid NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone,
                "IsDeleted" boolean NOT NULL DEFAULT false,
                CONSTRAINT "PK_LegalSections" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_LegalSections_Chambers_ChamberId" FOREIGN KEY ("ChamberId") REFERENCES "Chambers"("Id") ON DELETE RESTRICT
            );
            CREATE INDEX IF NOT EXISTS "IX_LegalSections_SectionCode" ON "LegalSections"("SectionCode");
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "WorkflowTemplates" (
                "Id" uuid NOT NULL,
                "ChamberId" uuid NOT NULL,
                "Name" character varying(255) NOT NULL,
                "Description" character varying(2000),
                "IsDefault" boolean NOT NULL DEFAULT false,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone,
                "IsDeleted" boolean NOT NULL DEFAULT false,
                CONSTRAINT "PK_WorkflowTemplates" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_WorkflowTemplates_Chambers_ChamberId" FOREIGN KEY ("ChamberId") REFERENCES "Chambers"("Id") ON DELETE CASCADE
            );
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "WorkflowTemplateSections" (
                "Id" uuid NOT NULL,
                "TemplateId" uuid NOT NULL,
                "LegalSectionId" uuid NOT NULL,
                "DisplayOrder" integer NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone,
                "IsDeleted" boolean NOT NULL DEFAULT false,
                CONSTRAINT "PK_WorkflowTemplateSections" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_WorkflowTemplateSections_WorkflowTemplates_TemplateId" FOREIGN KEY ("TemplateId") REFERENCES "WorkflowTemplates"("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_WorkflowTemplateSections_LegalSections_LegalSectionId" FOREIGN KEY ("LegalSectionId") REFERENCES "LegalSections"("Id") ON DELETE RESTRICT
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_WorkflowTemplateSections_TemplateId_LegalSectionId" ON "WorkflowTemplateSections"("TemplateId", "LegalSectionId");
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "CaseLegalSections" (
                "Id" uuid NOT NULL,
                "CaseId" uuid NOT NULL,
                "LegalSectionId" uuid NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone,
                "IsDeleted" boolean NOT NULL DEFAULT false,
                CONSTRAINT "PK_CaseLegalSections" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_CaseLegalSections_Cases_CaseId" FOREIGN KEY ("CaseId") REFERENCES "Cases"("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_CaseLegalSections_LegalSections_LegalSectionId" FOREIGN KEY ("LegalSectionId") REFERENCES "LegalSections"("Id") ON DELETE RESTRICT
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_CaseLegalSections_CaseId_LegalSectionId" ON "CaseLegalSections"("CaseId", "LegalSectionId");
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "LegalProcedures" (
                "Id" uuid NOT NULL,
                "LegalSectionId" uuid NOT NULL,
                "Name" character varying(500) NOT NULL,
                "Description" character varying(4000),
                "ProcedureType" character varying(100),
                "Timeline" character varying(1000),
                "RequiredDocuments" character varying(2000),
                "IsActive" boolean NOT NULL DEFAULT true,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone,
                "IsDeleted" boolean NOT NULL DEFAULT false,
                CONSTRAINT "PK_LegalProcedures" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_LegalProcedures_LegalSections_LegalSectionId" FOREIGN KEY ("LegalSectionId") REFERENCES "LegalSections"("Id") ON DELETE CASCADE
            );
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "CaseLegalProcedures" (
                "Id" uuid NOT NULL,
                "CaseLegalSectionId" uuid NOT NULL,
                "ProcedureName" character varying(500) NOT NULL,
                "Status" character varying(50) NOT NULL DEFAULT 'Pending',
                "StartedAt" timestamp with time zone,
                "CompletedAt" timestamp with time zone,
                "Notes" character varying(4000),
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone,
                "IsDeleted" boolean NOT NULL DEFAULT false,
                CONSTRAINT "PK_CaseLegalProcedures" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_CaseLegalProcedures_CaseLegalSections_CaseLegalSectionId" FOREIGN KEY ("CaseLegalSectionId") REFERENCES "CaseLegalSections"("Id") ON DELETE CASCADE
            );
            """);

        // Missing pre-Phase 7 tables (added to model but db was created before)
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "Leads" (
                "Id" uuid NOT NULL,
                "ChamberId" uuid NOT NULL,
                "Name" character varying(500) NOT NULL,
                "Phone" character varying(50) NOT NULL,
                "Email" character varying(320) NOT NULL,
                "CompanyName" character varying(500),
                "CaseType" character varying(200),
                "EstimatedValue" numeric NOT NULL DEFAULT 0,
                "LeadSource" character varying(200) NOT NULL DEFAULT '',
                "Stage" character varying(100) NOT NULL DEFAULT 'NewLead',
                "AssignedLawyerId" uuid,
                "Notes" text,
                "AttachmentsJson" text,
                "FollowUpDate" timestamp with time zone,
                "LastContactedAt" timestamp with time zone,
                "Score" integer NOT NULL DEFAULT 0,
                "ConvertedAt" timestamp with time zone NOT NULL DEFAULT '-infinity',
                "LostReason" character varying(1000),
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone,
                "IsDeleted" boolean NOT NULL DEFAULT false,
                CONSTRAINT "PK_Leads" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_Leads_Chambers_ChamberId" FOREIGN KEY ("ChamberId") REFERENCES "Chambers"("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_Leads_Users_AssignedLawyerId" FOREIGN KEY ("AssignedLawyerId") REFERENCES "Users"("Id") ON DELETE SET NULL
            );
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "TimeEntries" (
                "Id" uuid NOT NULL,
                "UserId" uuid NOT NULL,
                "ClientId" uuid,
                "CaseId" uuid,
                "TaskId" uuid,
                "InvoiceId" uuid,
                "Description" character varying(2000) NOT NULL DEFAULT '',
                "Category" character varying(100) NOT NULL DEFAULT 'General',
                "StartTime" timestamp with time zone NOT NULL,
                "EndTime" timestamp with time zone,
                "DurationMinutes" double precision NOT NULL DEFAULT 0,
                "HourlyRate" numeric NOT NULL DEFAULT 0,
                "Billable" boolean NOT NULL DEFAULT true,
                "Status" character varying(50) NOT NULL DEFAULT 'Running',
                "ChamberId" uuid NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone,
                "IsDeleted" boolean NOT NULL DEFAULT false,
                CONSTRAINT "PK_TimeEntries" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_TimeEntries_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users"("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_TimeEntries_Cases_CaseId" FOREIGN KEY ("CaseId") REFERENCES "Cases"("Id") ON DELETE SET NULL,
                CONSTRAINT "FK_TimeEntries_Tasks_TaskId" FOREIGN KEY ("TaskId") REFERENCES "Tasks"("Id") ON DELETE SET NULL,
                CONSTRAINT "FK_TimeEntries_Invoices_InvoiceId" FOREIGN KEY ("InvoiceId") REFERENCES "Invoices"("Id") ON DELETE SET NULL,
                CONSTRAINT "FK_TimeEntries_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "Clients"("Id") ON DELETE SET NULL,
                CONSTRAINT "FK_TimeEntries_Chambers_ChamberId" FOREIGN KEY ("ChamberId") REFERENCES "Chambers"("Id") ON DELETE CASCADE
            );
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "Messages" (
                "Id" uuid NOT NULL,
                "SenderId" uuid NOT NULL,
                "ReceiverId" uuid NOT NULL,
                "CaseId" uuid,
                "Content" text NOT NULL DEFAULT '',
                "AttachmentUrl" character varying(2000),
                "AttachmentFileName" character varying(500),
                "IsRead" boolean NOT NULL DEFAULT false,
                "ReadAt" timestamp with time zone,
                "IsClientVisible" boolean NOT NULL DEFAULT true,
                "CreatedAt" timestamp with time zone NOT NULL,
                "UpdatedAt" timestamp with time zone,
                "IsDeleted" boolean NOT NULL DEFAULT false,
                CONSTRAINT "PK_Messages" PRIMARY KEY ("Id"),
                CONSTRAINT "FK_Messages_Users_SenderId" FOREIGN KEY ("SenderId") REFERENCES "Users"("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_Messages_Users_ReceiverId" FOREIGN KEY ("ReceiverId") REFERENCES "Users"("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_Messages_Cases_CaseId" FOREIGN KEY ("CaseId") REFERENCES "Cases"("Id") ON DELETE SET NULL
            );
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
                UserId = AdminId,
                Plan = SubscriptionPlan.Chamber,
                Status = SubscriptionStatus.Active,
                CurrentPeriodStart = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CurrentPeriodEnd = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }

        if (!await db.Organizations.IgnoreQueryFilters().AnyAsync(o => o.Id == DefaultOrganizationId))
        {
            db.Organizations.Add(new Organization
            {
                Id = DefaultOrganizationId,
                Name = "Verdiq Admin Organization",
                Slug = "verdiq-admin",
                Description = "Default organization for Verdiq administration",
                OwnerId = AdminId,
                IsActive = true,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }

        if (!await db.OrganizationMembers.IgnoreQueryFilters()
            .AnyAsync(om => om.OrganizationId == DefaultOrganizationId && om.UserId == AdminId))
        {
            db.OrganizationMembers.Add(new OrganizationMember
            {
                Id = Guid.Parse("d0000001-0000-0000-0000-000000000001"),
                OrganizationId = DefaultOrganizationId,
                UserId = AdminId,
                Role = OrganizationRole.Owner,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }

        await db.SaveChangesAsync();
    }
}
