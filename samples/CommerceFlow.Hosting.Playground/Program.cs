using CommerceFlow.Hosting.Playground;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder =
    Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<HostLifecycleService>();
builder.Services.AddHostedService<HeartbeatWorker>();
builder.Services.AddHostedService<ScopedWorker>();
builder.Services.AddHostedService<AdvancedLifecycleService>();
builder.Services.AddHostedService<JobWorker>();

builder.Services.AddScoped<
    IScopedProcessingService,
    ScopedProcessingService>();

builder.Services.AddScoped<
    IJobProcessor,
    JobProcessor>();

string? configurationMessage =
    builder.Configuration["Application:Message"];

IHostEnvironment environment =
    builder.Environment;

IHost host = builder.Build();

IHostApplicationLifetime lifetime =
    host.Services
        .GetRequiredService<IHostApplicationLifetime>();

ILogger logger =
    host.Services
        .GetRequiredService<ILoggerFactory>()
        .CreateLogger("CommerceFlow.Hosting");

logger.LogInformation(
    "Application name: {ApplicationName}",
    environment.ApplicationName);

logger.LogInformation(
    "Environment: {EnvironmentName}",
    environment.EnvironmentName);

logger.LogInformation(
    "Content root: {ContentRootPath}",
    environment.ContentRootPath);

logger.LogInformation(
    "Is Development: {IsDevelopment}",
    environment.IsDevelopment());

logger.LogInformation(
    "Is Staging: {IsStaging}",
    environment.IsStaging());

logger.LogInformation(
    "Is Production: {IsProduction}",
    environment.IsProduction());

logger.LogInformation(
    "Configuration message: {ConfigurationMessage}",
    configurationMessage);

lifetime.ApplicationStarted.Register(() =>
{
    logger.LogInformation(
        "APPLICATION - Started");
});

lifetime.ApplicationStopping.Register(() =>
{
    logger.LogInformation(
        "APPLICATION - Stopping");
});

lifetime.ApplicationStopped.Register(() =>
{
    logger.LogInformation(
        "APPLICATION - Stopped");
});

await host.RunAsync();