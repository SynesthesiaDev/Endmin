// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Octokit;

namespace Endmin;

public static class Watcher
{
    private const string tiny_file = "./.endmin";

    private static readonly GitHubClient github_client = new(new ProductHeaderValue("Endmin"))
    {
        Credentials = new Credentials(ConfigurationManager.Current.GithubToken)
    };

    private static readonly Dictionary<string, string> current_sha_hash = new();

    public static void EnsureTinyFile()
    {
        if (!File.Exists(tiny_file))
        {
            Logger.Verbose("Tiny file not found, creating one..");
            File.Create(tiny_file).Close();
            File.WriteAllText(tiny_file, "");
        }

        var tiny = File.ReadAllText(tiny_file);
        var split = tiny.Split(",");
        foreach (var dict in split)
        {
            if (dict.Length == 0) continue;

            var dictSplit = dict.Split("=");
            var app = dictSplit[0];
            var sha = dictSplit[1];
            current_sha_hash[app] = sha;
        }
    }

    private static async Task updateTinyFile()
    {
        var encodedString = current_sha_hash.Aggregate("", (current, shaPair) => current + $"{shaPair.Key}={shaPair.Value},");
        await File.WriteAllTextAsync(tiny_file, encodedString);
    }

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
        Logger.Verbose("Polling version changes..");
        foreach (var app in ConfigurationManager.Current.Apps)
        {
            try
            {
                await checkAndUpdateApp(app);
            }
            catch (Exception ex)
            {
                Logger.Error($"[ {app.Name} ] Failed to check and update app", Logger.Network);
                Logger.Exception(ex, Logger.Network);
            }
        }
    }

    private static async Task checkAndUpdateApp(App app)
    {
        var branch = await github_client.Repository.Branch.Get(app.GithubUser, app.GithubRepo, app.GithubBranch);
        string latestSha = branch.Commit.Sha;

        if (!current_sha_hash.TryGetValue(app.Name, out var value) || latestSha != value)
        {
            if (!await isBuildFinished(app.GithubUser, app.GithubRepo, latestSha))
            {
                Logger.Debug($"[{app.Name}] New version found ({latestSha}) but image is not built yet", Logger.Network);
                return;
            }

            value = latestSha;
            await Deployment.DeployContainer(app, latestSha);

            current_sha_hash[app.Name] = value;
            await updateTinyFile();
        }
        else
        {
            Logger.Verbose($"No updates for {app.Name}", Logger.Network);
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
