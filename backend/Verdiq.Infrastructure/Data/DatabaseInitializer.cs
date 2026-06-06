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

        // Apply pending migrations to bring schema up to date.
        await db.Database.MigrateAsync();

        await EnsureDefaultDataAsync(db);
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
