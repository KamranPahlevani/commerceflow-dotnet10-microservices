using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommerceFlow.Hosting.Playground
{
    public class JobWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger _logger;

        public JobWorker(
            IServiceScopeFactory scopeFactory,
            ILogger<JobWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "JobWorker started");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await using AsyncServiceScope scope =
                        _scopeFactory.CreateAsyncScope();

                    IJobProcessor processor = 
                        scope.ServiceProvider
                        .GetRequiredService<IJobProcessor>();

                    await processor.ProcessAsync(
                        stoppingToken);

                    await Task.Delay(
                        TimeSpan.FromSeconds(5),
                        stoppingToken);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "JobWorker cancelation requested.");
            }

            _logger.LogInformation(
                "JobWorker Completed."
                );
        }
    }
}
