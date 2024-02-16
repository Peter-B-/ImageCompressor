using ImageCompressor;
using ImageCompressor.Service;
using Microsoft.Extensions.Options;
using Serilog;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
    .Build();

IHost host = Host.CreateDefaultBuilder(args)
    .UseContentRoot(AppContext.BaseDirectory)
    .ConfigureServices(services =>
    {
        services.AddOptions<SettingsRoot>()
            .BindConfiguration("Settings") // 👈 Bind the section
            .ValidateDataAnnotations() // 👈 Enable validation
            .ValidateOnStart(); // 👈 Validate on app start
        services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<SettingsRoot>>().Value);
    })
    .ConfigureServices(services => { services.AddHostedService<Worker>(); })
    .UseSerilog((context, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(configuration))
    .UseWindowsService()
    .Build();

host.Run();