namespace Endmin;

internal static class Endmin
{
    private static async Task Main(string[] args)
    {
        Logger.Verbose("Starting up Endmin..");
        Logger.Debug("Loading configuration..");
        ConfigurationManager.Load();
        Watcher.EnsureTinyFile();

        var cancellationTokenSource = new CancellationTokenSource();

        Console.CancelKeyPress += (s, e) => {
            e.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        await Watcher.StartAsync(cancellationTokenSource.Token);
    }
}
