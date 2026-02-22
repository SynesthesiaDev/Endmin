using System.Text;
using CsToml;

namespace Endmin;

public static class ConfigurationManager
{
    private const string path = "./config.toml";

    private static readonly CsTomlSerializerOptions options = CsTomlSerializerOptions.Default with
    {
        SerializeOptions = new SerializeOptions { ArrayStyle = TomlArrayStyle.Header }
    };

    public static readonly Configuration DEFAULT = new()
    {
        PollingInterval = 60000,
        GithubToken = "github_token_here",
        Apps =
        [
            new App
            {
                Name = "Test App",
                DockerRepository = "ghcr.io/SynesthesiaDev/test",
                ContainerName = "test-app",
                GithubBranch = "master",
                GithubRepo = "test",
                GithubUser = "SynesthesiaDev",
                InternalPort = 8080,
                ExternalPort = 8080
            }
        ]
    };

    public static Configuration Current = DEFAULT;

    public static void Load()
    {
        if (!File.Exists(path))
        {
            Logger.Debug("Configuration file not found, creating one..");
            File.Create(path).Close();
            var release = CsTomlSerializer.Serialize(DEFAULT, options);
            File.WriteAllText(path, release.ToString());
        }

        var text = File.ReadAllText(path);
        var decoded = CsTomlSerializer.Deserialize<Configuration>(Encoding.UTF8.GetBytes(text), options);
        Current = decoded;
        Logger.Debug("Configuration file loaded!");
    }
}
