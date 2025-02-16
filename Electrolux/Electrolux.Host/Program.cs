using Electrolux.Application;
using Electrolux.Application.Mqtt;
using Electrolux.Domain.Shared;
using Electrolux.Domain.UseCases.GetAppliances;
using Electrolux.Infrastructure.ApiClients.Electrolux;
using Electrolux.Infrastructure.ApiClients.Gigya;
using Electrolux.Infrastructure.Mqtt;
using Electrolux.Infrastructure.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, configuration) =>
    {
        configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, false)
            .AddJsonFile("appsettings.user.json", true, false)
            .AddUserSecrets(typeof(Program).Assembly, true, false)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<MqttOptions>(context.Configuration.GetSection("Mqtt"));
        services.Configure<ElectroluxOptions>(context.Configuration.GetSection("Electrolux"));
        services.Configure<ApplicationOptions>(context.Configuration.GetSection("Config"));

        services.AddSingleton<MqttService>();
        services.AddSingleton<IMqttControlService>(s => s.GetRequiredService<MqttService>());
        services.AddSingleton<IMqttPublishingService>(s => s.GetRequiredService<MqttService>());
        services.AddSingleton<IMqttSubscriptionService>(s => s.GetRequiredService<MqttService>());
        services.AddSingleton<ElectroluxApiClient>();
        services.AddSingleton<GigyaApiClient>();
        services.AddSingleton<ElectroluxRepository>();
        services.AddSingleton<ApplianceService>();

        services.AddHttpClient();
        services.AddHostedService<MqttWorker>();
    })
    .UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console())
    .Build();

Log.Logger.Information("Application Start");

await host.RunAsync();

Log.Logger.Information("Application Shutdown");