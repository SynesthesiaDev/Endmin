// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Serilog;
using SynesthesiaDev.Synx;
using SynesthesiaDev.Synx.Codon;

namespace Endmin;

public static class ConfigurationManager
{
    public static readonly string DATA_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
    public static readonly string CONFIG_PATH = Path.Combine(DATA_PATH, "config.synx");
    public static readonly string HASH_FILE = Path.Combine(DATA_PATH, "hashes.synx");

    public static readonly Configuration DEFAULT = new Configuration
    (
        PollingInterval: 60000,
        GithubToken: "github_token_here",
        Apps:
        [
            new App
            (
                Name: "Test App",
                ContainerName: "test-app",
                HostDataPath: null,
                ContainerDataPath: null,
                DockerRepository: "ghcr.io/SynesthesiaDev/test",
                GithubUser: "SynesthesiaDev",
                GithubRepo: "test",
                GithubBranch: "master",
                InternalPort: 8080,
                ExternalPort: 8080
            )
        ]
    );

    public static Configuration Current = DEFAULT;

    public static void Load()
    {
        Log.Verbose("Loading configuration..");
        Directory.CreateDirectory(DATA_PATH);

        if (!File.Exists(CONFIG_PATH))
        {
            Log.Debug("Configuration file not found, creating one..");
            File.Create(CONFIG_PATH).Close();
            var release = Configuration.CODEC.Encode(SynxTranscoder.INSTANCE, DEFAULT).Object().EncodeToString();
            File.WriteAllText(CONFIG_PATH, release);
        }

        var text = File.ReadAllText(CONFIG_PATH);
        var decoded = Configuration.CODEC.Decode(SynxTranscoder.INSTANCE, text.ToSynxObject());
        Current = decoded;
        Log.Debug("Configuration file loaded!");
    }
}
