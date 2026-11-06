using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public sealed class HeartbeatWorker : BackgroundService
{
    private readonly ILogger<HeartbeatWorker> _logger;

    public HeartbeatWorker(
        ILogger<HeartbeatWorker> logger)
    {
        _logger = logger;

        _logger.LogInformation(
            "HeartbeatWorker constructor.");
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "HeartbeatWorker ExecuteAsync started.");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "Heartbeat at {CurrentTime}",
                    DateTimeOffset.Now);

                await Task.Delay(
                    TimeSpan.FromSeconds(2),
                    stoppingToken);
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "HeartbeatWorker cancellation requested.");
        }

        _logger.LogInformation(
            "HeartbeatWorker ExecuteAsync completed.");
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "HeartbeatWorker StopAsync started.");

        await base.StopAsync(cancellationToken);

        _logger.LogInformation(
            "HeartbeatWorker StopAsync completed.");
    }
}