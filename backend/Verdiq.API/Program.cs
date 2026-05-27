using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Verdiq.API.Hubs;
using Verdiq.API.Middleware;
using Verdiq.API.Services;
using Verdiq.Application.Interfaces;
using Verdiq.Application.Validators;
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

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var path = context.HttpContext.Request.Path;
                    if (path.StartsWithSegments("/hubs"))
                    {
                        var token = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(token))
                            context.Token = token;
                    }
                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorization();

    // Domain services
    builder.Services.AddScoped<IJwtService, JwtService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IDashboardService, DashboardService>();

    // Application services
    builder.Services.AddScoped<IChamberService, ChamberService>();
    builder.Services.AddScoped<ICaseService, CaseService>();
    builder.Services.AddScoped<IClientService, ClientService>();
    builder.Services.AddScoped<IHearingService, HearingService>();
    builder.Services.AddScoped<IDocumentService, DocumentService>();
    builder.Services.AddScoped<ICloudStorageService, CloudStorageService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
    builder.Services.AddScoped<IInvoiceService, InvoiceService>();
    builder.Services.AddScoped<IExpenseService, ExpenseService>();
    builder.Services.AddScoped<ITaskService, TaskService>();
    builder.Services.AddScoped<ITemplateService, TemplateService>();
    builder.Services.AddScoped<IReminderService, ReminderService>();
    builder.Services.AddScoped<ILegalDocumentService, LegalDocumentService>();
    builder.Services.AddScoped<IPermissionService, PermissionService>();
    builder.Services.AddScoped<IAIService, AIService>();
    builder.Services.AddHttpClient<IAIService, AIService>();
    builder.Services.AddScoped<IAdminService, AdminService>();
    builder.Services.AddScoped<ISearchService, SearchService>();
    builder.Services.AddScoped<ISuperAdminService, SuperAdminService>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IOrganizationService, OrganizationService>();

    builder.Services.AddSignalR();
    builder.Services.AddScoped<IRealtimeNotifier, RealtimeNotifier>();

    builder.Services.AddHealthChecks();

    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var path = context.Request.Path;
            if (path.StartsWithSegments("/hubs") || path.StartsWithSegments("/health"))
                return RateLimitPartition.GetNoLimiter("realtime");

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ??
                              context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 5
                });
        });
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

    app.UseMiddleware<SecurityHeadersMiddleware>();
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
    app.MapHub<NotificationHub>("/hubs/notifications");
    app.MapHub<PresenceHub>("/hubs/presence");

    if (!app.Environment.IsEnvironment("Testing"))
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await DatabaseInitializer.InitializeAsync(db);
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

static bool IsTestingEnvironment() =>
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.Equals("Testing", StringComparison.OrdinalIgnoreCase) == true;
