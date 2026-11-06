using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public sealed class AdvancedLifecycleService
    : IHostedLifecycleService
{
    private readonly ILogger<AdvancedLifecycleService> _logger;

    public AdvancedLifecycleService(
        ILogger<AdvancedLifecycleService> logger)
    {
        _logger = logger;

        _logger.LogInformation(
            "AdvancedLifecycleService constructor.");
    }

    public Task StartingAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("1 - StartingAsync");

        return Task.CompletedTask;
    }

    public Task StartAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("2 - StartAsync");

        return Task.CompletedTask;
    }

    public Task StartedAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("3 - StartedAsync");

        return Task.CompletedTask;
    }

    public Task StoppingAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("4 - StoppingAsync");

        return Task.CompletedTask;
    }

    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("5 - StopAsync");

        return Task.CompletedTask;
    }

    public Task StoppedAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("6 - StoppedAsync");

        return Task.CompletedTask;
    }
}