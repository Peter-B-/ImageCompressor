using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace ImageCompressor.Service;

public class Worker : BackgroundService
{
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            disposables.Dispose();
        }
    }

    public sealed override void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private readonly ILogger<Worker> logger;
    private readonly SettingsRoot settings;

    private readonly CompositeDisposable disposables = new();

    public Worker(ILogger<Worker> logger, SettingsRoot settings)
    {
        this.logger = logger;
        this.settings = settings;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var task in settings.CompressionTasks)
        {
            logger.LogInformation("Setting up task {TaskName}...", task.Name);
            var validationResult = task.CompressionSettings.Validate();
            logger.LogInformation("Validation result: {ValidationResult}: {ValidationMessage}", validationResult.Successful, validationResult.Message);
            if (validationResult.Successful)
            {
                var subscrition = Observable.Timer(task.InitialDelay, task.Period)
                    .Do(_ => logger.LogInformation("Executing task {TaskName}", task.Name))
                    .Do(_ => (new CompressImagesCommand()).ExecuteEmbedded(logger, task.CompressionSettings))
                    .Retry()
                    .Subscribe(_ => {}, exception => logger.LogError(exception, "Error while executing task {TaskName}", task.Name));
                disposables.Add(subscrition);

            }
        }
        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(-1, stoppingToken);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        disposables?.Dispose();
        return base.StopAsync(cancellationToken);
    }
}