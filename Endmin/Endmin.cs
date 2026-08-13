// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SpectreConsole;

namespace Endmin;

internal static class Endmin
{
    private static async Task Main(string[] args)
    {
        GCSettings.LatencyMode = GCLatencyMode.Batch;

        await using var log = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.SpectreConsole(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u4}] {Message:lj}{NewLine}{Exception}", minLevel: LogEventLevel.Verbose)
            .CreateLogger();

        Log.Logger = log;

        Log.Verbose("Starting up Endmin..");
        ConfigurationManager.Load();

        HashesFile.ReadFile();

        var dockerExists = await Deployment.IsDockerRunningAsync();
        if (!dockerExists)
        {
            Log.Error("Docker is not running! Cannot use Endmin, exiting..");
            Environment.Exit(1);
        }

        var cancellationTokenSource = new CancellationTokenSource();

        Console.CancelKeyPress += (_, e) => {
            e.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        await Watcher.StartAsync(cancellationTokenSource.Token);
    }
}
