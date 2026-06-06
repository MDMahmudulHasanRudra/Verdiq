CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "AuditLogs" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Action" character varying(255) NOT NULL,
        "Entity" character varying(100) NOT NULL,
        "EntityId" character varying(100),
        "OldValues" text,
        "NewValues" text,
        "IpAddress" character varying(50) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "CauseLists" (
        "Id" uuid NOT NULL,
        "CourtName" character varying(255) NOT NULL,
        "CaseNumber" character varying(50) NOT NULL,
        "HearingDate" timestamp with time zone NOT NULL,
        "Status" character varying(20) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_CauseLists" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "Chambers" (
        "Id" uuid NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Logo" character varying(500),
        "Address" character varying(500),
        "Phone" character varying(20),
        "SubscriptionPlan" character varying(20) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Chambers" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "LegalDocuments" (
        "Id" uuid NOT NULL,
        "Title" character varying(500) NOT NULL,
        "Category" character varying(20) NOT NULL,
        "Content" text NOT NULL,
        "Citation" character varying(255),
        "JudgeName" character varying(255),
        "Keywords" character varying(1000),
        "Year" integer,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_LegalDocuments" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "Permissions" (
        "Id" uuid NOT NULL,
        "Name" character varying(100) NOT NULL,
        "Description" character varying(500) NOT NULL,
        "Module" character varying(100) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Permissions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "Templates" (
        "Id" uuid NOT NULL,
        "Title" character varying(255) NOT NULL,
        "Category" character varying(100) NOT NULL,
        "Content" text NOT NULL,
        "Variables" character varying(2000),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Templates" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "LegalSections" (
        "Id" uuid NOT NULL,
        "SectionCode" character varying(50) NOT NULL,
        "SectionTitle" character varying(500) NOT NULL,
        "LawName" character varying(255) NOT NULL,
        "Country" character varying(100),
        "Category" character varying(100),
        "Description" character varying(4000),
        "Severity" character varying(20),
        "IsActive" boolean NOT NULL,
        "ChamberId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_LegalSections" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_LegalSections_Chambers_ChamberId" FOREIGN KEY ("ChamberId") REFERENCES "Chambers" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "RolePermissions" (
        "Id" uuid NOT NULL,
        "Role" character varying(20) NOT NULL,
        "PermissionId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_RolePermissions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_RolePermissions_Permissions_PermissionId" FOREIGN KEY ("PermissionId") REFERENCES "Permissions" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "LegalProcedures" (
        "Id" uuid NOT NULL,
        "LegalSectionId" uuid NOT NULL,
        "StepNumber" integer NOT NULL,
        "Title" character varying(500) NOT NULL,
        "Description" character varying(4000),
        "RequiredDocuments" character varying(2000),
        "RecommendedTimeline" character varying(200),
        "ResponsibleRole" character varying(50),
        "IsMandatory" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_LegalProcedures" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_LegalProcedures_LegalSections_LegalSectionId" FOREIGN KEY ("LegalSectionId") REFERENCES "LegalSections" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "AiConversations" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Role" character varying(20) NOT NULL,
        "Content" character varying(10000) NOT NULL,
        "TokensUsed" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_AiConversations" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "CaseActivities" (
        "Id" uuid NOT NULL,
        "CaseId" uuid NOT NULL,
        "ActivityType" character varying(20) NOT NULL,
        "Description" character varying(2000) NOT NULL,
        "CreatedBy" uuid NOT NULL,
        "IsClientVisible" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_CaseActivities" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "CaseLegalProcedures" (
        "Id" uuid NOT NULL,
        "CaseLegalSectionId" uuid NOT NULL,
        "LegalProcedureId" uuid NOT NULL,
        "IsCompleted" boolean NOT NULL,
        "CompletedAt" timestamp with time zone,
        "CompletedBy" character varying(255),
        "Notes" character varying(2000),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_CaseLegalProcedures" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_CaseLegalProcedures_LegalProcedures_LegalProcedureId" FOREIGN KEY ("LegalProcedureId") REFERENCES "LegalProcedures" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "CaseLegalSections" (
        "Id" uuid NOT NULL,
        "CaseId" uuid NOT NULL,
        "LegalSectionId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_CaseLegalSections" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_CaseLegalSections_LegalSections_LegalSectionId" FOREIGN KEY ("LegalSectionId") REFERENCES "LegalSections" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "Cases" (
        "Id" uuid NOT NULL,
        "Title" character varying(500) NOT NULL,
        "CaseNumber" character varying(50) NOT NULL,
        "CourtName" character varying(255) NOT NULL,
        "CaseType" character varying(100) NOT NULL,
        "FilingDate" timestamp with time zone NOT NULL,
        "Opponent" character varying(255),
        "Status" character varying(20) NOT NULL,
        "Priority" character varying(20) NOT NULL,
        "Description" character varying(4000),
        "ActsAndSections" character varying(500),
        "FirNumber" character varying(50),
        "PoliceStation" character varying(100),
        "ClosingDate" timestamp with time zone,
        "GdNumber" character varying(50),
        "JudgeName" character varying(255),
        "Bench" character varying(100),
        "Prosecutor" character varying(255),
        "OpposingLawyer" character varying(255),
        "Jurisdiction" character varying(100),
        "AppealStatus" character varying(50),
        "RiskLevel" character varying(20),
        "ComplexityScore" integer,
        "PracticeArea" character varying(100),
        "Department" character varying(100),
        "InternalNotes" character varying(4000),
        "RetainerAmount" numeric,
        "BillingMethod" character varying(50),
        "FixedFee" numeric,
        "HourlyRate" numeric,
        "BudgetLimit" numeric,
        "ExpenseBudget" numeric,
        "NextHearingDate" timestamp with time zone,
        "CriticalDeadlines" character varying(2000),
        "LimitationExpiry" timestamp with time zone,
        "CaseTemplateId" uuid,
        "AssignedLawyerId" uuid NOT NULL,
        "ChamberId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Cases" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Cases_Chambers_ChamberId" FOREIGN KEY ("ChamberId") REFERENCES "Chambers" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "Hearings" (
        "Id" uuid NOT NULL,
        "CaseId" uuid NOT NULL,
        "HearingDate" timestamp with time zone NOT NULL,
        "Courtroom" character varying(50),
        "JudgeName" character varying(255),
        "Result" character varying(2000),
        "NextHearingDate" timestamp with time zone,
        "Status" character varying(20) NOT NULL,
        "Notes" character varying(2000),
        "ReminderSent" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Hearings" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Hearings_Cases_CaseId" FOREIGN KEY ("CaseId") REFERENCES "Cases" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "ChamberSettings" (
        "Id" uuid NOT NULL,
        "ChamberId" uuid NOT NULL,
        "SettingsJson" jsonb NOT NULL,
        "UpdatedBy" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ChamberSettings" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ChamberSettings_Chambers_ChamberId" FOREIGN KEY ("ChamberId") REFERENCES "Chambers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "ClientCases" (
        "Id" uuid NOT NULL,
        "ClientId" uuid NOT NULL,
        "CaseId" uuid NOT NULL,
        "Role" character varying(50),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_ClientCases" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_ClientCases_Cases_CaseId" FOREIGN KEY ("CaseId") REFERENCES "Cases" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "Clients" (
        "Id" uuid NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Phone" character varying(20) NOT NULL,
        "Email" character varying(255) NOT NULL,
        "Address" character varying(500),
        "Nid" character varying(50),
        "CompanyName" character varying(255),
        "Notes" character varying(4000),
        "IsActive" boolean NOT NULL,
        "ClientType" character varying(50),
        "ClientCode" character varying(50),
        "PassportNumber" character varying(50),
        "DateOfBirth" timestamp with time zone,
        "Gender" character varying(20),
        "Occupation" character varying(100),
        "Nationality" character varying(100),
        "TradeLicense" character varying(100),
        "RegistrationNumber" character varying(100),
        "TaxVatNumber" character varying(100),
        "AuthorizedRepresentative" character varying(255),
        "Tags" character varying(1000),
        "RiskLevel" character varying(20),
        "ClientCategory" character varying(100),
        "BillingPreference" character varying(50),
        "PaymentTerms" character varying(200),
        "CreditLimit" numeric,
        "PreferredContactMethod" character varying(50),
        "WhatsAppNumber" character varying(20),
        "SecondaryPhone" character varying(20),
        "EmergencyContact" character varying(255),
        "IsBlacklisted" boolean NOT NULL,
        "UserId" uuid,
        "ChamberId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Clients" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Clients_Chambers_ChamberId" FOREIGN KEY ("ChamberId") REFERENCES "Chambers" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "Invoices" (
        "Id" uuid NOT NULL,
        "InvoiceNumber" character varying(50) NOT NULL,
        "Amount" numeric(18,2) NOT NULL,
        "Currency" character varying(10) NOT NULL,
        "Status" character varying(20) NOT NULL,
        "Description" character varying(2000),
        "DueDate" timestamp with time zone,
        "PaidAt" timestamp with time zone,
        "ClientId" uuid NOT NULL,
        "CaseId" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Invoices" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Invoices_Cases_CaseId" FOREIGN KEY ("CaseId") REFERENCES "Cases" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Invoices_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "Clients" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "FullName" character varying(255) NOT NULL,
        "Email" character varying(255) NOT NULL,
        "Phone" character varying(20) NOT NULL,
        "PasswordHash" text NOT NULL,
        "Role" character varying(20) NOT NULL,
        "IsActive" boolean NOT NULL,
        "Status" character varying(20),
        "AvatarUrl" character varying(500),
        "BarCouncilId" character varying(50),
        "RefreshToken" character varying(500),
        "RefreshTokenExpiry" timestamp with time zone,
        "TwoFactorEnabled" boolean NOT NULL,
        "TwoFactorSecret" character varying(256),
        "TwoFactorVerifiedAt" timestamp with time zone,
        "LoginAttempts" integer NOT NULL,
        "LockoutEnd" timestamp with time zone,
        "LastLoginAt" timestamp with time zone,
        "LastLoginIp" character varying(50),
        "ClientId" uuid,
        "ChamberId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Users_Chambers_ChamberId" FOREIGN KEY ("ChamberId") REFERENCES "Chambers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Users_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "Clients" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "Documents" (
        "Id" uuid NOT NULL,
        "FileName" character varying(500) NOT NULL,
        "OriginalFileName" character varying(500) NOT NULL,
        "FilePath" character varying(1000) NOT NULL,
        "FileType" character varying(100) NOT NULL,
        "FileSize" bigint NOT NULL,
        "Category" character varying(100) NOT NULL,
        "FolderPath" character varying(500),
        "Status" character varying(20) NOT NULL,
        "Version" integer NOT NULL,
        "CaseId" uuid NOT NULL,
        "UploadedById" uuid NOT NULL,
        "StorageProvider" character varying(50) NOT NULL,
        "StorageKey" character varying(500),
        "Visibility" character varying(20) NOT NULL,
        "SharedWithClientId" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Documents" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Documents_Cases_CaseId" FOREIGN KEY ("CaseId") REFERENCES "Cases" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Documents_Clients_SharedWithClientId" FOREIGN KEY ("SharedWithClientId") REFERENCES "Clients" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Documents_Users_UploadedById" FOREIGN KEY ("UploadedById") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "Expenses" (
        "Id" uuid NOT NULL,
        "Description" character varying(500) NOT NULL,
        "Amount" numeric(18,2) NOT NULL,
        "Currency" character varying(10) NOT NULL,
        "Category" character varying(100) NOT NULL,
        "ExpenseDate" timestamp with time zone NOT NULL,
        "ReceiptPath" character varying(1000),
        "ChamberId" uuid NOT NULL,
        "CaseId" uuid,
        "UserId" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Expenses" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Expenses_Cases_CaseId" FOREIGN KEY ("CaseId") REFERENCES "Cases" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Expenses_Chambers_ChamberId" FOREIGN KEY ("ChamberId") REFERENCES "Chambers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Expenses_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "Leads" (
        "Id" uuid NOT NULL,
        "ChamberId" uuid NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Phone" character varying(20) NOT NULL,
        "Email" character varying(255) NOT NULL,
        "CompanyName" character varying(255),
        "CaseType" character varying(100),
        "EstimatedValue" numeric(18,2) NOT NULL,
        "LeadSource" character varying(50) NOT NULL,
        "Stage" character varying(30) NOT NULL,
        "AssignedLawyerId" uuid,
        "Notes" character varying(4000),
        "AttachmentsJson" character varying(4000),
        "FollowUpDate" timestamp with time zone,
        "LastContactedAt" timestamp with time zone,
        "Score" integer NOT NULL,
        "ConvertedAt" timestamp with time zone NOT NULL,
        "LostReason" character varying(500),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Leads" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Leads_Chambers_ChamberId" FOREIGN KEY ("ChamberId") REFERENCES "Chambers" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Leads_Users_AssignedLawyerId" FOREIGN KEY ("AssignedLawyerId") REFERENCES "Users" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "Messages" (
        "Id" uuid NOT NULL,
        "SenderId" uuid NOT NULL,
        "ReceiverId" uuid NOT NULL,
        "CaseId" uuid,
        "Content" character varying(5000) NOT NULL,
        "AttachmentUrl" character varying(1000),
        "AttachmentFileName" character varying(500),
        "IsRead" boolean NOT NULL,
        "ReadAt" timestamp with time zone,
        "IsClientVisible" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Messages" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Messages_Cases_CaseId" FOREIGN KEY ("CaseId") REFERENCES "Cases" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Messages_Users_ReceiverId" FOREIGN KEY ("ReceiverId") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Messages_Users_SenderId" FOREIGN KEY ("SenderId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "Notifications" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Title" character varying(255) NOT NULL,
        "Message" character varying(2000) NOT NULL,
        "Type" character varying(50) NOT NULL,
        "IsRead" boolean NOT NULL,
        "ReferenceId" character varying(100),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Notifications" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Notifications_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "Organizations" (
        "Id" uuid NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Slug" character varying(100) NOT NULL,
        "Description" character varying(2000) NOT NULL,
        "LogoUrl" character varying(500),
        "Website" character varying(255),
        "Address" character varying(500),
        "Phone" character varying(20),
        "Email" character varying(255),
        "IsActive" boolean NOT NULL,
        "OwnerId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Organizations" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Organizations_Users_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "Reminders" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "ChamberId" uuid NOT NULL,
        "Type" character varying(50) NOT NULL,
        "Channel" character varying(20) NOT NULL,
        "Priority" character varying(20) NOT NULL,
        "Title" character varying(255) NOT NULL,
        "Message" character varying(2000) NOT NULL,
        "RelatedEntityType" character varying(50),
        "RelatedEntityId" uuid,
        "ScheduledAt" timestamp with time zone NOT NULL,
        "SentStatus" boolean NOT NULL,
        "SentAt" timestamp with time zone,
        "ReferenceId" uuid,
        "Status" character varying(20) NOT NULL,
        "ReadAt" timestamp with time zone,
        "CompletedAt" timestamp with time zone,
        "SnoozedUntil" timestamp with time zone,
        "EscalationLevel" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Reminders" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Reminders_Chambers_ChamberId" FOREIGN KEY ("ChamberId") REFERENCES "Chambers" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Reminders_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "Subscriptions" (
        "Id" uuid NOT NULL,
        "ChamberId" uuid NOT NULL,
        "UserId" uuid,
        "Plan" character varying(20) NOT NULL,
        "Status" character varying(20) NOT NULL,
        "CurrentPeriodStart" timestamp with time zone NOT NULL,
        "CurrentPeriodEnd" timestamp with time zone NOT NULL,
        "CancelAtPeriodEnd" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Subscriptions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Subscriptions_Chambers_ChamberId" FOREIGN KEY ("ChamberId") REFERENCES "Chambers" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Subscriptions_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "Tasks" (
        "Id" uuid NOT NULL,
        "Title" character varying(255) NOT NULL,
        "Description" character varying(2000) NOT NULL,
        "DueDate" timestamp with time zone NOT NULL,
        "Status" character varying(20) NOT NULL,
        "Priority" character varying(20),
        "AssignedTo" uuid NOT NULL,
        "AssignedBy" uuid NOT NULL,
        "CaseId" uuid,
        "ChamberId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Tasks" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Tasks_Cases_CaseId" FOREIGN KEY ("CaseId") REFERENCES "Cases" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Tasks_Chambers_ChamberId" FOREIGN KEY ("ChamberId") REFERENCES "Chambers" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Tasks_Users_AssignedBy" FOREIGN KEY ("AssignedBy") REFERENCES "Users" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Tasks_Users_AssignedTo" FOREIGN KEY ("AssignedTo") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "DocumentContents" (
        "Id" uuid NOT NULL,
        "DocumentId" uuid NOT NULL,
        "ExtractedText" character varying(50000) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_DocumentContents" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_DocumentContents_Documents_DocumentId" FOREIGN KEY ("DocumentId") REFERENCES "Documents" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "DocumentVersions" (
        "Id" uuid NOT NULL,
        "VersionNumber" integer NOT NULL,
        "FileName" character varying(500) NOT NULL,
        "OriginalFileName" character varying(500) NOT NULL,
        "FilePath" character varying(1000) NOT NULL,
        "FileType" character varying(100) NOT NULL,
        "FileSize" bigint NOT NULL,
        "ChangeNotes" character varying(2000),
        "Status" character varying(20) NOT NULL,
        "StorageProvider" character varying(50) NOT NULL,
        "StorageKey" character varying(500),
        "DocumentId" uuid NOT NULL,
        "UploadedById" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_DocumentVersions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_DocumentVersions_Documents_DocumentId" FOREIGN KEY ("DocumentId") REFERENCES "Documents" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_DocumentVersions_Users_UploadedById" FOREIGN KEY ("UploadedById") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "OrganizationMembers" (
        "Id" uuid NOT NULL,
        "OrganizationId" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "Role" character varying(20) NOT NULL,
        "InvitedEmail" character varying(255),
        "InvitedAt" timestamp with time zone,
        "AcceptedAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_OrganizationMembers" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_OrganizationMembers_Organizations_OrganizationId" FOREIGN KEY ("OrganizationId") REFERENCES "Organizations" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_OrganizationMembers_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "Workspaces" (
        "Id" uuid NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" character varying(2000),
        "Color" character varying(20),
        "OrganizationId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Workspaces" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Workspaces_Organizations_OrganizationId" FOREIGN KEY ("OrganizationId") REFERENCES "Organizations" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "Payments" (
        "Id" uuid NOT NULL,
        "InvoiceNumber" text NOT NULL,
        "Amount" numeric(18,2) NOT NULL,
        "Currency" character varying(3) NOT NULL,
        "PaymentMethod" character varying(20) NOT NULL,
        "Status" character varying(20) NOT NULL,
        "Gateway" character varying(20),
        "GatewayReference" text,
        "TransactionId" character varying(100),
        "PhoneNumber" character varying(20),
        "PaidAt" timestamp with time zone,
        "Description" character varying(2000),
        "ClientId" uuid NOT NULL,
        "InvoiceId" uuid,
        "SubscriptionId" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Payments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Payments_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "Clients" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_Payments_Invoices_InvoiceId" FOREIGN KEY ("InvoiceId") REFERENCES "Invoices" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_Payments_Subscriptions_SubscriptionId" FOREIGN KEY ("SubscriptionId") REFERENCES "Subscriptions" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE TABLE "TimeEntries" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "ClientId" uuid,
        "CaseId" uuid,
        "TaskId" uuid,
        "InvoiceId" uuid,
        "Description" character varying(2000) NOT NULL,
        "Category" character varying(100) NOT NULL,
        "StartTime" timestamp with time zone NOT NULL,
        "EndTime" timestamp with time zone,
        "DurationMinutes" float NOT NULL,
        "HourlyRate" numeric(18,2) NOT NULL,
        "Billable" boolean NOT NULL,
        "Status" character varying(20) NOT NULL,
        "ChamberId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_TimeEntries" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_TimeEntries_Cases_CaseId" FOREIGN KEY ("CaseId") REFERENCES "Cases" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_TimeEntries_Chambers_ChamberId" FOREIGN KEY ("ChamberId") REFERENCES "Chambers" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_TimeEntries_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "Clients" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_TimeEntries_Invoices_InvoiceId" FOREIGN KEY ("InvoiceId") REFERENCES "Invoices" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_TimeEntries_Tasks_TaskId" FOREIGN KEY ("TaskId") REFERENCES "Tasks" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_TimeEntries_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    INSERT INTO "Chambers" ("Id", "Address", "CreatedAt", "IsDeleted", "Logo", "Name", "Phone", "SubscriptionPlan", "UpdatedAt")
    VALUES ('c0000000-0000-0000-0000-000000000001', '42 Gulshan Avenue, Dhaka', TIMESTAMPTZ '2024-01-01T00:00:00Z', FALSE, NULL, 'Verdiq Chamber', '+8801700000000', 'Chamber', NULL);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000001', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Create cases', FALSE, 'Cases', 'case.create', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000002', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'View cases', FALSE, 'Cases', 'case.view', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000003', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Edit cases', FALSE, 'Cases', 'case.edit', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000004', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Delete cases', FALSE, 'Cases', 'case.delete', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000005', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Create clients', FALSE, 'Clients', 'client.create', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000006', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'View clients', FALSE, 'Clients', 'client.view', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000007', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Edit clients', FALSE, 'Clients', 'client.edit', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000008', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Delete clients', FALSE, 'Clients', 'client.delete', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000009', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Upload documents', FALSE, 'Documents', 'document.upload', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000010', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'View documents', FALSE, 'Documents', 'document.view', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000011', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Delete documents', FALSE, 'Documents', 'document.delete', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000012', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Create hearings', FALSE, 'Hearings', 'hearing.create', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000013', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'View hearings', FALSE, 'Hearings', 'hearing.view', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000014', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Edit hearings', FALSE, 'Hearings', 'hearing.edit', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000015', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Create invoices', FALSE, 'Billing', 'invoice.create', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000016', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'View invoices', FALSE, 'Billing', 'invoice.view', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000017', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Assign tasks', FALSE, 'Tasks', 'task.assign', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000018', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'View tasks', FALSE, 'Tasks', 'task.view', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000019', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'View reports', FALSE, 'Reports', 'report.view', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000020', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Manage settings', FALSE, 'Settings', 'settings.manage', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000021', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Create legal sections', FALSE, 'Legal', 'legalsection.create', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000022', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'View legal sections', FALSE, 'Legal', 'legalsection.view', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000023', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Edit legal sections', FALSE, 'Legal', 'legalsection.edit', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000024', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Delete legal sections', FALSE, 'Legal', 'legalsection.delete', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000025', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Create legal procedures', FALSE, 'Legal', 'legalprocedure.create', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000026', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'View legal procedures', FALSE, 'Legal', 'legalprocedure.view', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000027', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Edit legal procedures', FALSE, 'Legal', 'legalprocedure.edit', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000028', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Delete legal procedures', FALSE, 'Legal', 'legalprocedure.delete', NULL);
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000029', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Manage chamber configuration', FALSE, 'Configuration', 'configuration.manage', NULL);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    INSERT INTO "Users" ("Id", "AvatarUrl", "BarCouncilId", "ChamberId", "ClientId", "CreatedAt", "Email", "FullName", "IsActive", "IsDeleted", "LastLoginAt", "LastLoginIp", "LockoutEnd", "LoginAttempts", "PasswordHash", "Phone", "RefreshToken", "RefreshTokenExpiry", "Role", "Status", "TwoFactorEnabled", "TwoFactorSecret", "TwoFactorVerifiedAt", "UpdatedAt")
    VALUES ('a1b2c3d4-e5f6-7890-abcd-ef1234567890', NULL, NULL, 'c0000000-0000-0000-0000-000000000001', NULL, TIMESTAMPTZ '2024-01-01T00:00:00Z', 'admin@verdiq.com', 'Admin Verdiq', TRUE, FALSE, NULL, NULL, NULL, 0, '$2a$11$VyGwoqxHC6gMQ9iMsda/7eE9a5TV9SOHBRyX4SgwU.RJNNxnYEera', '+8801700000000', NULL, NULL, 'Owner', NULL, FALSE, NULL, NULL, NULL);
    INSERT INTO "Users" ("Id", "AvatarUrl", "BarCouncilId", "ChamberId", "ClientId", "CreatedAt", "Email", "FullName", "IsActive", "IsDeleted", "LastLoginAt", "LastLoginIp", "LockoutEnd", "LoginAttempts", "PasswordHash", "Phone", "RefreshToken", "RefreshTokenExpiry", "Role", "Status", "TwoFactorEnabled", "TwoFactorSecret", "TwoFactorVerifiedAt", "UpdatedAt")
    VALUES ('e5f6a7b8-c9d0-1234-5678-9abcdef01234', NULL, 'BC-2024-001', 'c0000000-0000-0000-0000-000000000001', NULL, TIMESTAMPTZ '2024-01-15T00:00:00Z', 'lawyer@verdiq.com', 'Adv. Abdul Karim', TRUE, FALSE, NULL, NULL, NULL, 0, '$2a$11$CnI9Ur82n8LPzJkcFCD6Q.D4J892KK5RHTh7BAXnHCmKE3cQOxOey', '+8801712345678', NULL, NULL, 'SeniorLawyer', NULL, FALSE, NULL, NULL, NULL);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    INSERT INTO "Subscriptions" ("Id", "CancelAtPeriodEnd", "ChamberId", "CreatedAt", "CurrentPeriodEnd", "CurrentPeriodStart", "IsDeleted", "Plan", "Status", "UpdatedAt", "UserId")
    VALUES ('b2c3d4e5-f6a7-8901-bcde-f12345678901', FALSE, 'c0000000-0000-0000-0000-000000000001', TIMESTAMPTZ '2024-01-01T00:00:00Z', TIMESTAMPTZ '2025-01-01T00:00:00Z', TIMESTAMPTZ '2024-01-01T00:00:00Z', FALSE, 'Chamber', 'Active', NULL, 'a1b2c3d4-e5f6-7890-abcd-ef1234567890');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_AiConversations_UserId" ON "AiConversations" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_AuditLogs_CreatedAt" ON "AuditLogs" ("CreatedAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_CaseActivities_CaseId" ON "CaseActivities" ("CaseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE UNIQUE INDEX "IX_CaseLegalProcedures_CaseLegalSectionId_LegalProcedureId" ON "CaseLegalProcedures" ("CaseLegalSectionId", "LegalProcedureId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_CaseLegalProcedures_LegalProcedureId" ON "CaseLegalProcedures" ("LegalProcedureId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE UNIQUE INDEX "IX_CaseLegalSections_CaseId_LegalSectionId" ON "CaseLegalSections" ("CaseId", "LegalSectionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_CaseLegalSections_LegalSectionId" ON "CaseLegalSections" ("LegalSectionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Cases_AssignedLawyerId" ON "Cases" ("AssignedLawyerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE UNIQUE INDEX "IX_Cases_CaseNumber" ON "Cases" ("CaseNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Cases_ChamberId" ON "Cases" ("ChamberId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE UNIQUE INDEX "IX_ChamberSettings_ChamberId" ON "ChamberSettings" ("ChamberId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_ChamberSettings_UpdatedBy" ON "ChamberSettings" ("UpdatedBy");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_ClientCases_CaseId" ON "ClientCases" ("CaseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE UNIQUE INDEX "IX_ClientCases_ClientId_CaseId" ON "ClientCases" ("ClientId", "CaseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Clients_ChamberId" ON "Clients" ("ChamberId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE UNIQUE INDEX "IX_Clients_ClientCode" ON "Clients" ("ClientCode") WHERE [ClientCode] IS NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Clients_Email" ON "Clients" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Clients_Phone" ON "Clients" ("Phone");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Clients_UserId" ON "Clients" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_DocumentContents_DocumentId" ON "DocumentContents" ("DocumentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Documents_CaseId" ON "Documents" ("CaseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Documents_SharedWithClientId" ON "Documents" ("SharedWithClientId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Documents_UploadedById" ON "Documents" ("UploadedById");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_DocumentVersions_DocumentId" ON "DocumentVersions" ("DocumentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_DocumentVersions_UploadedById" ON "DocumentVersions" ("UploadedById");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Expenses_CaseId" ON "Expenses" ("CaseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Expenses_ChamberId" ON "Expenses" ("ChamberId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Expenses_UserId" ON "Expenses" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Hearings_CaseId" ON "Hearings" ("CaseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Hearings_HearingDate" ON "Hearings" ("HearingDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Invoices_CaseId" ON "Invoices" ("CaseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Invoices_ClientId" ON "Invoices" ("ClientId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE UNIQUE INDEX "IX_Invoices_InvoiceNumber" ON "Invoices" ("InvoiceNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Leads_AssignedLawyerId" ON "Leads" ("AssignedLawyerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Leads_ChamberId" ON "Leads" ("ChamberId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_LegalProcedures_LegalSectionId" ON "LegalProcedures" ("LegalSectionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_LegalSections_ChamberId" ON "LegalSections" ("ChamberId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_LegalSections_SectionCode" ON "LegalSections" ("SectionCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Messages_CaseId" ON "Messages" ("CaseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Messages_ReceiverId" ON "Messages" ("ReceiverId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Messages_SenderId" ON "Messages" ("SenderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Notifications_UserId_IsRead" ON "Notifications" ("UserId", "IsRead");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE UNIQUE INDEX "IX_OrganizationMembers_OrganizationId_UserId" ON "OrganizationMembers" ("OrganizationId", "UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_OrganizationMembers_UserId" ON "OrganizationMembers" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Organizations_OwnerId" ON "Organizations" ("OwnerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE UNIQUE INDEX "IX_Organizations_Slug" ON "Organizations" ("Slug");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Payments_ClientId" ON "Payments" ("ClientId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Payments_InvoiceId" ON "Payments" ("InvoiceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Payments_SubscriptionId" ON "Payments" ("SubscriptionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE UNIQUE INDEX "IX_Permissions_Name" ON "Permissions" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Reminders_ChamberId" ON "Reminders" ("ChamberId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Reminders_UserId" ON "Reminders" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_RolePermissions_PermissionId" ON "RolePermissions" ("PermissionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE UNIQUE INDEX "IX_RolePermissions_Role_PermissionId" ON "RolePermissions" ("Role", "PermissionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Subscriptions_ChamberId" ON "Subscriptions" ("ChamberId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE UNIQUE INDEX "IX_Subscriptions_UserId" ON "Subscriptions" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Tasks_AssignedBy" ON "Tasks" ("AssignedBy");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Tasks_AssignedTo" ON "Tasks" ("AssignedTo");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Tasks_CaseId" ON "Tasks" ("CaseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Tasks_ChamberId" ON "Tasks" ("ChamberId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_TimeEntries_CaseId" ON "TimeEntries" ("CaseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_TimeEntries_ChamberId" ON "TimeEntries" ("ChamberId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_TimeEntries_ClientId" ON "TimeEntries" ("ClientId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_TimeEntries_InvoiceId" ON "TimeEntries" ("InvoiceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_TimeEntries_TaskId" ON "TimeEntries" ("TaskId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_TimeEntries_UserId" ON "TimeEntries" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Users_ChamberId" ON "Users" ("ChamberId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Users_ClientId" ON "Users" ("ClientId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    CREATE INDEX "IX_Workspaces_OrganizationId" ON "Workspaces" ("OrganizationId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    ALTER TABLE "AiConversations" ADD CONSTRAINT "FK_AiConversations_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    ALTER TABLE "CaseActivities" ADD CONSTRAINT "FK_CaseActivities_Cases_CaseId" FOREIGN KEY ("CaseId") REFERENCES "Cases" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    ALTER TABLE "CaseLegalProcedures" ADD CONSTRAINT "FK_CaseLegalProcedures_CaseLegalSections_CaseLegalSectionId" FOREIGN KEY ("CaseLegalSectionId") REFERENCES "CaseLegalSections" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    ALTER TABLE "CaseLegalSections" ADD CONSTRAINT "FK_CaseLegalSections_Cases_CaseId" FOREIGN KEY ("CaseId") REFERENCES "Cases" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    ALTER TABLE "Cases" ADD CONSTRAINT "FK_Cases_Users_AssignedLawyerId" FOREIGN KEY ("AssignedLawyerId") REFERENCES "Users" ("Id") ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    ALTER TABLE "ChamberSettings" ADD CONSTRAINT "FK_ChamberSettings_Users_UpdatedBy" FOREIGN KEY ("UpdatedBy") REFERENCES "Users" ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    ALTER TABLE "ClientCases" ADD CONSTRAINT "FK_ClientCases_Clients_ClientId" FOREIGN KEY ("ClientId") REFERENCES "Clients" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    ALTER TABLE "Clients" ADD CONSTRAINT "FK_Clients_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528115443_AddChamberSettings') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260528115443_AddChamberSettings', '10.0.0-preview.2.25163.8');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528120152_AddWorkflowTemplates') THEN
    ALTER TABLE "Cases" ADD "WorkflowTemplateId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528120152_AddWorkflowTemplates') THEN
    CREATE TABLE "WorkflowTemplates" (
        "Id" uuid NOT NULL,
        "ChamberId" uuid NOT NULL,
        "Name" character varying(255) NOT NULL,
        "Description" character varying(2000),
        "IsDefault" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_WorkflowTemplates" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WorkflowTemplates_Chambers_ChamberId" FOREIGN KEY ("ChamberId") REFERENCES "Chambers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528120152_AddWorkflowTemplates') THEN
    CREATE TABLE "WorkflowTemplateSections" (
        "Id" uuid NOT NULL,
        "TemplateId" uuid NOT NULL,
        "LegalSectionId" uuid NOT NULL,
        "DisplayOrder" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_WorkflowTemplateSections" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_WorkflowTemplateSections_LegalSections_LegalSectionId" FOREIGN KEY ("LegalSectionId") REFERENCES "LegalSections" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_WorkflowTemplateSections_WorkflowTemplates_TemplateId" FOREIGN KEY ("TemplateId") REFERENCES "WorkflowTemplates" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528120152_AddWorkflowTemplates') THEN
    INSERT INTO "Permissions" ("Id", "CreatedAt", "Description", "IsDeleted", "Module", "Name", "UpdatedAt")
    VALUES ('f0000000-0000-0000-0000-000000000030', TIMESTAMPTZ '2024-01-01T00:00:00Z', 'Manage workflow templates', FALSE, 'Workflow', 'workflow.manage', NULL);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528120152_AddWorkflowTemplates') THEN
    CREATE INDEX "IX_Cases_WorkflowTemplateId" ON "Cases" ("WorkflowTemplateId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528120152_AddWorkflowTemplates') THEN
    CREATE INDEX "IX_WorkflowTemplates_ChamberId" ON "WorkflowTemplates" ("ChamberId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528120152_AddWorkflowTemplates') THEN
    CREATE INDEX "IX_WorkflowTemplateSections_LegalSectionId" ON "WorkflowTemplateSections" ("LegalSectionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528120152_AddWorkflowTemplates') THEN
    CREATE UNIQUE INDEX "IX_WorkflowTemplateSections_TemplateId_LegalSectionId" ON "WorkflowTemplateSections" ("TemplateId", "LegalSectionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528120152_AddWorkflowTemplates') THEN
    ALTER TABLE "Cases" ADD CONSTRAINT "FK_Cases_WorkflowTemplates_WorkflowTemplateId" FOREIGN KEY ("WorkflowTemplateId") REFERENCES "WorkflowTemplates" ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260528120152_AddWorkflowTemplates') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260528120152_AddWorkflowTemplates', '10.0.0-preview.2.25163.8');
    END IF;
END $EF$;
COMMIT;

