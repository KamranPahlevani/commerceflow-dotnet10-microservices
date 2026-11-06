using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommerceFlow.Hosting.Playground
{
    public interface IJobProcessor
    {
        Task ProcessAsync(CancellationToken cancellationToken);
    }

    public class JobProcessor : IJobProcessor
    {
        private readonly ILogger<JobProcessor> _logger;
        private readonly Guid _instanceId = Guid.NewGuid();

        public JobProcessor(ILogger<JobProcessor> logger)
        {
            _logger = logger;

            _logger.LogInformation(
                "JobProcessor created. InstanceId: {instanceId}",
                _instanceId
                );
        }
        public async Task ProcessAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "JobProcessor created. InstanceId: {instanceId}",
                _instanceId
                );

            await Task.Delay(TimeSpan.FromSeconds(1),
                cancellationToken);
        }
    }
}
