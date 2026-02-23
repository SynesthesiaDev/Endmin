// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime;

namespace Endmin;

internal static class Endmin
{
    private static async Task Main(string[] args)
    {
        GCSettings.LatencyMode = GCLatencyMode.Batch;

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
