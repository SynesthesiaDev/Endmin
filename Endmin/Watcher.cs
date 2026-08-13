// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Octokit;
using Serilog;

namespace Endmin;

public static class Watcher
{
    private static readonly GitHubClient github_client = new(new ProductHeaderValue("Endmin"))
    {
        Credentials = new Credentials(ConfigurationManager.Current.GithubToken)
    };

    public static async Task StartAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(ConfigurationManager.Current.PollingInterval));

        await poll();
        while (await timer.WaitForNextTickAsync(ct))
        {
            await poll();
        }
    }

    private static async Task poll()
    {
        foreach (var app in ConfigurationManager.Current.Apps)
        {
            try
            {
                await checkAndUpdateApp(app);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[ {AppName} ] Failed to check and update app", app.Name);
            }
        }
        GC.Collect(2, GCCollectionMode.Aggressive, blocking: true, compacting: true);
    }

    private static async Task checkAndUpdateApp(App app)
    {
        var branch = await github_client.Repository.Branch.Get(app.GithubUser, app.GithubRepo, app.GithubBranch);
        string latestSha = branch.Commit.Sha;

        if (!HashesFile.Hashes.TryGetValue(app.Name, out var value) || latestSha != value)
        {
            if (!await isBuildFinished(app.GithubUser, app.GithubRepo, latestSha))
            {
                Log.Debug("[{AppName}] New version found ({LatestSha}) but image is not built yet", app.Name, latestSha);
                return;
            }

            value = latestSha;
            await Deployment.DeployContainer(app, latestSha);

            HashesFile.Hashes[app.Name] = value;
            HashesFile.WriteFile();
            GC.Collect(2, GCCollectionMode.Aggressive, blocking: true, compacting: true);
        }
        else
        {
            Log.Verbose("No updates for {AppName}", app.Name);
        }
    }

    private static async Task<bool> isBuildFinished(string owner, string repo, string sha)
    {
        var checkRuns = await github_client.Check.Run.GetAllForReference(owner, repo, sha);

        if (checkRuns.TotalCount == 0) return true;

        return checkRuns.CheckRuns.All(x =>
            x.Status == CheckStatus.Completed &&
            x.Conclusion == CheckConclusion.Success);
    }
}
