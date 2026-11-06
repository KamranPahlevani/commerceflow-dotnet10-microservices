using Microsoft.Extensions.Logging;

public interface IScopedProcessingService
{
    Task ProcessAsync(
        CancellationToken cancellationToken);
}

public sealed class ScopedProcessingService
    : IScopedProcessingService
{
    private readonly ILogger<ScopedProcessingService> _logger;
    private readonly Guid _instanceId = Guid.NewGuid();

    public ScopedProcessingService(
        ILogger<ScopedProcessingService> logger)
    {
        _logger = logger;

        _logger.LogInformation(
            "ScopedProcessingService created. InstanceId: {InstanceId}",
            _instanceId);
    }

    public async Task ProcessAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing with scoped instance {InstanceId}",
            _instanceId);

        await Task.Delay(
            TimeSpan.FromSeconds(1),
            cancellationToken);
    }
}