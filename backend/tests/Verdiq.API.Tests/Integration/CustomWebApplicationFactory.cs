using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Infrastructure.Data;
using Task = System.Threading.Tasks.Task;

namespace Verdiq.API.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("verdiq_test")
        .WithUsername("test")
        .WithPassword("test1234")
        .WithCleanUp(true)
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            var connStr = _container.GetConnectionString();
            connStr += ";Include Error Detail=true";
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connStr));

            services.AddTransient<IStartupFilter, DbMigrationFilter>();
        });
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

public class DbMigrationFilter : IStartupFilter
{
    private readonly AppDbContext _db;

    public DbMigrationFilter(AppDbContext db)
    {
        _db = db;
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        _db.Database.EnsureCreated();
        SeedTestData();
        return next;
    }

    private void SeedTestData()
    {
        if (_db.Users.Any()) return;

        var chamberId = Guid.Parse("c0000000-0000-0000-0000-000000000001");
        var adminId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        var adminSubId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");
        var lawyerId = Guid.Parse("e5f6a7b8-c9d0-1234-5678-9abcdef01234");
        var lawyerSubId = Guid.Parse("f6a7b8c9-d0e1-2345-6789-abcdef012345");

        _db.Chambers.Add(new Chamber
        {
            Id = chamberId,
            Name = "Test Chamber",
            Address = "42 Gulshan Avenue, Dhaka",
            Phone = "+8801700000000",
            SubscriptionPlan = SubscriptionPlan.Chamber,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        _db.Users.AddRange(
            new User
            {
                Id = adminId,
                FullName = "Admin Verdiq",
                Email = "admin@verdiq.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Phone = "+8801700000000",
                Role = UserRole.Owner,
                IsActive = true,
                ChamberId = chamberId,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new User
            {
                Id = lawyerId,
                FullName = "Adv. Abdul Karim",
                Email = "lawyer@verdiq.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("lawyer123"),
                Phone = "+8801712345678",
                BarCouncilId = "BC-2024-001",
                Role = UserRole.SeniorLawyer,
                IsActive = true,
                ChamberId = chamberId,
                CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        _db.Subscriptions.AddRange(
            new Subscription
            {
                Id = adminSubId,
                ChamberId = chamberId,
                Plan = SubscriptionPlan.Chamber,
                Status = SubscriptionStatus.Active,
                CurrentPeriodStart = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CurrentPeriodEnd = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Subscription
            {
                Id = lawyerSubId,
                ChamberId = chamberId,
                Plan = SubscriptionPlan.Pro,
                Status = SubscriptionStatus.Active,
                CurrentPeriodStart = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                CurrentPeriodEnd = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        _db.SaveChanges();
    }
}
