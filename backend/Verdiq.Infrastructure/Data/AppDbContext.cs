using Microsoft.EntityFrameworkCore;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;

namespace Verdiq.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Chamber> Chambers => Set<Chamber>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<Case> Cases => Set<Case>();
    public DbSet<CaseActivity> CaseActivities => Set<CaseActivity>();
    public DbSet<CauseList> CauseLists => Set<CauseList>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<ClientCase> ClientCases => Set<ClientCase>();
    public DbSet<Hearing> Hearings => Set<Hearing>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public DbSet<DocumentContent> DocumentContents => Set<DocumentContent>();
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<Domain.Entities.Task> Tasks => Set<Domain.Entities.Task>();
    public DbSet<LegalDocument> LegalDocuments => Set<LegalDocument>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<AiConversation> AiConversations => Set<AiConversation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Chamber>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Chambers");
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Logo).HasMaxLength(500);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.SubscriptionPlan).HasConversion<string>().HasMaxLength(20);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Users");
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FullName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.BarCouncilId).HasMaxLength(50);
            entity.Property(e => e.RefreshToken).HasMaxLength(500);
            entity.Property(e => e.TwoFactorSecret).HasMaxLength(256);
            entity.Property(e => e.LastLoginIp).HasMaxLength(50);

            entity.HasOne(e => e.Chamber)
                .WithMany(c => c.Users)
                .HasForeignKey(e => e.ChamberId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Subscription)
                .WithOne(s => s.User)
                .HasForeignKey<Subscription>(s => s.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Permissions");
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Module).HasMaxLength(100);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("RolePermissions");
            entity.HasIndex(e => new { e.Role, e.PermissionId }).IsUnique();
            entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Case>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Cases");
            entity.HasIndex(e => e.CaseNumber).IsUnique();
            entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
            entity.Property(e => e.CaseNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CourtName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.CaseType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Opponent).HasMaxLength(255);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Priority).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(4000);
            entity.Property(e => e.ActsAndSections).HasMaxLength(500);
            entity.Property(e => e.FirNumber).HasMaxLength(50);
            entity.Property(e => e.PoliceStation).HasMaxLength(100);

            entity.HasOne(e => e.AssignedLawyer)
                .WithMany(u => u.AssignedCases)
                .HasForeignKey(e => e.AssignedLawyerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Chamber)
                .WithMany(c => c.Cases)
                .HasForeignKey(e => e.ChamberId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<CaseActivity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("CaseActivities");
            entity.Property(e => e.ActivityType).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(2000).IsRequired();

            entity.HasOne(e => e.Case)
                .WithMany(c => c.Activities)
                .HasForeignKey(e => e.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<CauseList>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("CauseLists");
            entity.Property(e => e.CourtName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.CaseNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Clients");
            entity.HasIndex(e => e.Phone);
            entity.HasIndex(e => e.Email);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Nid).HasMaxLength(50);
            entity.Property(e => e.CompanyName).HasMaxLength(255);
            entity.Property(e => e.Notes).HasMaxLength(2000);

            entity.HasOne(e => e.Chamber)
                .WithMany(c => c.Clients)
                .HasForeignKey(e => e.ChamberId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<ClientCase>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("ClientCases");
            entity.HasIndex(e => new { e.ClientId, e.CaseId }).IsUnique();

            entity.HasOne(e => e.Client)
                .WithMany(c => c.ClientCases)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Case)
                .WithMany(c => c.ClientCases)
                .HasForeignKey(e => e.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Hearing>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Hearings");
            entity.HasIndex(e => e.HearingDate);
            entity.Property(e => e.Courtroom).HasMaxLength(50);
            entity.Property(e => e.JudgeName).HasMaxLength(255);
            entity.Property(e => e.Result).HasMaxLength(2000);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Notes).HasMaxLength(2000);

            entity.HasOne(e => e.Case)
                .WithMany(c => c.Hearings)
                .HasForeignKey(e => e.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Documents");
            entity.Property(e => e.FileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.OriginalFileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.FileType).HasMaxLength(100);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.FolderPath).HasMaxLength(500);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.StorageProvider).HasMaxLength(50);
            entity.Property(e => e.StorageKey).HasMaxLength(500);

            entity.HasOne(e => e.Case)
                .WithMany(c => c.Documents)
                .HasForeignKey(e => e.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UploadedBy)
                .WithMany(u => u.UploadedDocuments)
                .HasForeignKey(e => e.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Versions)
                .WithOne(v => v.Document)
                .HasForeignKey(v => v.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<DocumentVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("DocumentVersions");
            entity.Property(e => e.FileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.OriginalFileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.FileType).HasMaxLength(100);
            entity.Property(e => e.ChangeNotes).HasMaxLength(2000);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.StorageProvider).HasMaxLength(50);
            entity.Property(e => e.StorageKey).HasMaxLength(500);

            entity.HasOne(e => e.UploadedBy)
                .WithMany()
                .HasForeignKey(e => e.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<DocumentContent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("DocumentContents");
            entity.Property(e => e.ExtractedText).HasMaxLength(50000);

            entity.HasOne(e => e.Document)
                .WithMany()
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Template>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Templates");
            entity.Property(e => e.Title).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Variables).HasMaxLength(2000);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Invoices");
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            entity.Property(e => e.InvoiceNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(2000);

            entity.HasOne(e => e.Client)
                .WithMany(c => c.Invoices)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Case)
                .WithMany()
                .HasForeignKey(e => e.CaseId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Expenses");
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Category).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ReceiptPath).HasMaxLength(1000);

            entity.HasOne(e => e.Chamber)
                .WithMany()
                .HasForeignKey(e => e.ChamberId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Case)
                .WithMany()
                .HasForeignKey(e => e.CaseId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Payments");
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            entity.Property(e => e.InvoiceNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.PaymentMethod).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Gateway).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.GatewayReference).HasMaxLength(255);
            entity.Property(e => e.TransactionId).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(2000);

            entity.HasOne(e => e.Client)
                .WithMany(c => c.Payments)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Invoice)
                .WithMany()
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Subscriptions");
            entity.Property(e => e.Plan).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(e => e.Chamber)
                .WithMany()
                .HasForeignKey(e => e.ChamberId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Reminder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Reminders");
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Channel).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Message).HasMaxLength(2000).IsRequired();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Domain.Entities.Task>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Tasks");
            entity.Property(e => e.Title).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Priority).HasMaxLength(20);

            entity.HasOne(e => e.AssignedUser)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(e => e.AssignedTo)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Assigner)
                .WithMany()
                .HasForeignKey(e => e.AssignedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Case)
                .WithMany()
                .HasForeignKey(e => e.CaseId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Chamber)
                .WithMany()
                .HasForeignKey(e => e.ChamberId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<LegalDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("LegalDocuments");
            entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Category).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Citation).HasMaxLength(255);
            entity.Property(e => e.JudgeName).HasMaxLength(255);
            entity.Property(e => e.Keywords).HasMaxLength(1000);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Notifications");
            entity.HasIndex(e => new { e.UserId, e.IsRead });
            entity.Property(e => e.Title).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ReferenceId).HasMaxLength(100);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("AuditLogs");
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.Action).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Entity).HasMaxLength(100);
            entity.Property(e => e.EntityId).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<AiConversation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("AiConversations");
            entity.Property(e => e.Role).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Content).HasMaxLength(10000).IsRequired();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Organizations");
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.Website).HasMaxLength(255);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(255);

            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<OrganizationMember>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("OrganizationMembers");
            entity.HasIndex(e => new { e.OrganizationId, e.UserId }).IsUnique();
            entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.InvitedEmail).HasMaxLength(255);

            entity.HasOne(e => e.Organization)
                .WithMany(o => o.Members)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Workspace>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Workspaces");
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Color).HasMaxLength(20);

            entity.HasOne(e => e.Organization)
                .WithMany(o => o.Workspaces)
                .HasForeignKey(e => e.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var chamberId = Guid.Parse("c0000000-0000-0000-0000-000000000001");
        var adminId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        var lawyerId = Guid.Parse("e5f6a7b8-c9d0-1234-5678-9abcdef01234");
        var subId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");

        modelBuilder.Entity<Chamber>().HasData(new Chamber
        {
            Id = chamberId,
            Name = "Verdiq Chamber",
            Address = "42 Gulshan Avenue, Dhaka",
            Phone = "+8801700000000",
            SubscriptionPlan = SubscriptionPlan.Chamber,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = adminId,
            FullName = "Admin Verdiq",
            Email = "admin@verdiq.com",
            PasswordHash = SeedPasswords.Admin,
            Phone = "+8801700000000",
            Role = UserRole.Owner,
            IsActive = true,
            ChamberId = chamberId,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = lawyerId,
            FullName = "Adv. Abdul Karim",
            Email = "lawyer@verdiq.com",
            PasswordHash = SeedPasswords.Lawyer,
            Phone = "+8801712345678",
            BarCouncilId = "BC-2024-001",
            Role = UserRole.SeniorLawyer,
            IsActive = true,
            ChamberId = chamberId,
            CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<Subscription>().HasData(new Subscription
        {
            Id = subId,
            ChamberId = chamberId,
            UserId = adminId,
            Plan = SubscriptionPlan.Chamber,
            Status = SubscriptionStatus.Active,
            CurrentPeriodStart = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CurrentPeriodEnd = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        SeedPermissions(modelBuilder);
    }

    private static void SeedPermissions(ModelBuilder modelBuilder)
    {
        var perms = new[]
        {
            ("case.create", "Create cases", "Cases"),
            ("case.view", "View cases", "Cases"),
            ("case.edit", "Edit cases", "Cases"),
            ("case.delete", "Delete cases", "Cases"),
            ("client.create", "Create clients", "Clients"),
            ("client.view", "View clients", "Clients"),
            ("client.edit", "Edit clients", "Clients"),
            ("client.delete", "Delete clients", "Clients"),
            ("document.upload", "Upload documents", "Documents"),
            ("document.view", "View documents", "Documents"),
            ("document.delete", "Delete documents", "Documents"),
            ("hearing.create", "Create hearings", "Hearings"),
            ("hearing.view", "View hearings", "Hearings"),
            ("hearing.edit", "Edit hearings", "Hearings"),
            ("invoice.create", "Create invoices", "Billing"),
            ("invoice.view", "View invoices", "Billing"),
            ("task.assign", "Assign tasks", "Tasks"),
            ("task.view", "View tasks", "Tasks"),
            ("report.view", "View reports", "Reports"),
            ("settings.manage", "Manage settings", "Settings")
        };

        var id = Guid.Parse("f0000000-0000-0000-0000-000000000001");
        var permissions = new List<Permission>();
        for (int i = 0; i < perms.Length; i++)
        {
            permissions.Add(new Permission
            {
                Id = Guid.Parse($"f0000000-0000-0000-0000-{i + 1:D12}"),
                Name = perms[i].Item1,
                Description = perms[i].Item2,
                Module = perms[i].Item3,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }
        modelBuilder.Entity<Permission>().HasData(permissions);
    }
}
