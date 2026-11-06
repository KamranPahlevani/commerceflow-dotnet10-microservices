using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public sealed class ScopedWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScopedWorker> _logger;

    public ScopedWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<ScopedWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "ScopedWorker started.");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await using AsyncServiceScope scope =
                    _scopeFactory.CreateAsyncScope();

                IScopedProcessingService processingService =
                    scope.ServiceProvider
                        .GetRequiredService<IScopedProcessingService>();

                await processingService.ProcessAsync(
                    stoppingToken);

                await Task.Delay(
                    TimeSpan.FromSeconds(2),
                    stoppingToken);
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "ScopedWorker cancellation requested.");
        }

        _logger.LogInformation(
            "ScopedWorker completed.");
    }
}