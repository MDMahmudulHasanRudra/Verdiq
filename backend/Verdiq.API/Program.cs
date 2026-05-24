using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Verdiq.API.Middleware;
using Verdiq.Application.Interfaces;
using Verdiq.Application.Validators;
using Verdiq.Domain.Entities;
using Verdiq.Domain.Enums;
using Verdiq.Domain.Interfaces;
using Verdiq.Infrastructure.Data;
using Verdiq.Infrastructure.Repositories;
using Verdiq.Infrastructure.Services;

try
{
    var builder = WebApplication.CreateBuilder(args);

    if (!builder.Environment.IsEnvironment("Testing"))
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/verdiq-.log", rollingInterval: RollingInterval.Day)
            .CreateBootstrapLogger();

        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/verdiq-.log", rollingInterval: RollingInterval.Day);
        });
    }

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "Verdiq API", Version = "v1" });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT Authorization header using the Bearer scheme"
        });

        c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
        {
            { new OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
        });
    });

    builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
        if (!builder.Environment.IsEnvironment("Testing"))
            options.AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
    });

    if (!builder.Environment.IsEnvironment("Testing"))
        builder.Services.AddScoped<AuditSaveChangesInterceptor>();
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ??
                        "VerdiqSecretKey2024SuperSecureLongKey!@#$%^&*()")),
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "Verdiq",
                ValidateAudience = true,
                ValidAudience = builder.Configuration["Jwt:Audience"] ?? "VerdiqApp",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();

    builder.Services.AddScoped<IJwtService, JwtService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IDashboardService, DashboardService>();
    builder.Services.AddScoped<ICaseService, CaseService>();
    builder.Services.AddScoped<IHearingService, HearingService>();
    builder.Services.AddScoped<IClientService, ClientService>();
    builder.Services.AddScoped<IDocumentService, DocumentService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    builder.Services.AddHealthChecks();

    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 5
                }));
        options.RejectionStatusCode = 429;
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.SetIsOriginAllowed(_ => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    var app = builder.Build();

    if (!app.Environment.IsEnvironment("Testing"))
        app.UseSerilogRequestLogging();

    app.UseMiddleware<ExceptionMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();

    app.UseRateLimiter();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("AllowFrontend");
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    if (!app.Environment.IsEnvironment("Testing"))
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        }
    }

    app.Run();
}
catch (Exception ex)
{
    if (!IsTestingEnvironment())
        Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    if (!IsTestingEnvironment())
        Log.CloseAndFlush();
}

static void SeedDefaultUsers(AppDbContext db)
{
    if (db.Users.Any()) return;

    var adminId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
    var adminSubId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");
    var lawyerId = Guid.Parse("e5f6a7b8-c9d0-1234-5678-9abcdef01234");
    var lawyerSubId = Guid.Parse("f6a7b8c9-d0e1-2345-6789-abcdef012345");

    db.Users.AddRange(
        new User
        {
            Id = adminId,
            FullName = "Admin Verdiq",
            Email = "admin@verdiq.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Phone = "+8801700000000",
            Role = UserRole.Admin,
            IsActive = true,
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
            ChamberAddress = "42 Gulshan Avenue, Dhaka",
            Role = UserRole.Lawyer,
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
        }
    );

    db.Subscriptions.AddRange(
        new Subscription
        {
            Id = adminSubId,
            UserId = adminId,
            Plan = SubscriptionPlan.Chamber,
            Status = SubscriptionStatus.Active,
            CurrentPeriodStart = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CurrentPeriodEnd = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        },
        new Subscription
        {
            Id = lawyerSubId,
            UserId = lawyerId,
            Plan = SubscriptionPlan.Pro,
            Status = SubscriptionStatus.Active,
            CurrentPeriodStart = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            CurrentPeriodEnd = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
        }
    );

    db.SaveChanges();
}

static bool IsTestingEnvironment() =>
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.Equals("Testing", StringComparison.OrdinalIgnoreCase) == true;
