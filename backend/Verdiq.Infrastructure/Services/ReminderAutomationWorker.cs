using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Verdiq.Application.Interfaces;

namespace Verdiq.Infrastructure.Services;

public class ReminderAutomationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReminderAutomationWorker> _logger;
    private const int CheckIntervalSeconds = 300; // 5 minutes

    public ReminderAutomationWorker(IServiceScopeFactory scopeFactory, ILogger<ReminderAutomationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reminder automation worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();
                await reminderService.EvaluateAllChambersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating automation rules");
            }

            await Task.Delay(TimeSpan.FromSeconds(CheckIntervalSeconds), stoppingToken);
        }
    }
}
