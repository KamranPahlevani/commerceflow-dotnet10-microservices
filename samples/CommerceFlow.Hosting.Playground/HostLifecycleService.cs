using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public sealed class HostLifecycleService : IHostedService
{
    private readonly ILogger<HostLifecycleService> _logger;

    public HostLifecycleService(
        ILogger<HostLifecycleService> logger)
    {
        _logger = logger;

        _logger.LogInformation(
            "HostLifecycleService constructor.");
    }

    public Task StartAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "HostLifecycleService StartAsync.");

        return Task.CompletedTask;
    }

    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "HostLifecycleService StopAsync.");

        return Task.CompletedTask;
    }
}