using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;

namespace Verdiq.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Case> Cases => Set<Case>();
    public DbSet<Hearing> Hearings => Set<Hearing>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Users");
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.BarCouncilId).HasMaxLength(50);
            entity.Property(e => e.ChamberAddress).HasMaxLength(500);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.RefreshToken).HasMaxLength(500);

            entity.HasOne(e => e.Subscription)
                .WithOne(s => s.User)
                .HasForeignKey<Subscription>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Clients");
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Phone);
            entity.HasIndex(e => e.NationalId);
            entity.Property(e => e.FullName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Address).HasMaxLength(500).IsRequired();
            entity.Property(e => e.NationalId).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(2000);

            entity.HasOne(e => e.AssignedLawyer)
                .WithMany()
                .HasForeignKey(e => e.AssignedLawyerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Case>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Cases");
            entity.HasIndex(e => e.CaseNumber).IsUnique();
            entity.HasIndex(e => e.ClientId);
            entity.HasIndex(e => e.AssignedLawyerId);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.CaseNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
            entity.Property(e => e.CaseType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Priority).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Court).HasMaxLength(255).IsRequired();
            entity.Property(e => e.CourtRoom).HasMaxLength(50);
            entity.Property(e => e.JudgeName).HasMaxLength(255);
            entity.Property(e => e.FirNumber).HasMaxLength(50);
            entity.Property(e => e.PoliceStation).HasMaxLength(100);
            entity.Property(e => e.ActsAndSections).HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(4000);

            entity.HasOne(e => e.Client)
                .WithMany(c => c.Cases)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AssignedLawyer)
                .WithMany(u => u.AssignedCases)
                .HasForeignKey(e => e.AssignedLawyerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Hearing>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Hearings");
            entity.HasIndex(e => e.HearingDate);
            entity.HasIndex(e => e.CaseId);
            entity.Property(e => e.Time).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Court).HasMaxLength(255).IsRequired();
            entity.Property(e => e.CourtRoom).HasMaxLength(50);
            entity.Property(e => e.JudgeName).HasMaxLength(255);
            entity.Property(e => e.HearingType).HasMaxLength(100).IsRequired();
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
            entity.HasIndex(e => e.CaseId);
            entity.Property(e => e.FileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.OriginalFileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DocumentType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(e => e.Case)
                .WithMany(c => c.Documents)
                .HasForeignKey(e => e.CaseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UploadedBy)
                .WithMany(u => u.UploadedDocuments)
                .HasForeignKey(e => e.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);

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

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Subscriptions");
            entity.Property(e => e.Plan).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("Payments");
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            entity.Property(e => e.InvoiceNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(e => e.Currency).HasMaxLength(10).IsRequired();
            entity.Property(e => e.PaymentMethod).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.TransactionId).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);

            entity.HasOne(e => e.Subscription)
                .WithMany(s => s.Payments)
                .HasForeignKey(e => e.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Client)
                .WithMany(c => c.Payments)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("AuditLogs");
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Action).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Entity).HasMaxLength(100);
            entity.Property(e => e.EntityId).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(50);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var adminId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        var adminSubId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");
        var lawyerId = Guid.Parse("e5f6a7b8-c9d0-1234-5678-9abcdef01234");
        var lawyerSubId = Guid.Parse("f6a7b8c9-d0e1-2345-6789-abcdef012345");

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = adminId,
            FullName = "Admin Verdiq",
            Email = "admin@verdiq.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Phone = "+8801700000000",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<Subscription>().HasData(new Subscription
        {
            Id = adminSubId,
            UserId = adminId,
            Plan = SubscriptionPlan.Chamber,
            Status = SubscriptionStatus.Active,
            CurrentPeriodStart = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CurrentPeriodEnd = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = lawyerId,
            FullName = "Adv. Abdul Karim",
            Email = "lawyer@verdiq.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("lawyer123"),
            Phone = "+8801712345678",
            BarCouncilId = "BC-2024-001",
            ChamberAddress = "42 Gulshan Avenue, Dhaka",
            Role = UserRole.Lawyer,
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<Subscription>().HasData(new Subscription
        {
            Id = lawyerSubId,
            UserId = lawyerId,
            Plan = SubscriptionPlan.Pro,
            Status = SubscriptionStatus.Active,
            CurrentPeriodStart = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            CurrentPeriodEnd = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
