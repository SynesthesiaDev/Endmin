// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using Docker.DotNet;
using Docker.DotNet.Models;
using Serilog;

namespace Endmin;

public static class Deployment
{
    public static readonly DockerClient DOCKER_CLIENT = new DockerClientConfiguration().CreateClient();

    public static async Task DeployContainer(App app, string sha)
    {
        var image = $"{app.DockerRepository}:{sha}";
        Log.Debug("[{AppName}] Updating image {Image}..", app.Name, image);

        await DOCKER_CLIENT.Images.CreateImageAsync(new ImagesCreateParameters { FromImage = app.DockerRepository, Tag = sha }, null, new Progress<JSONMessage>());

        try
        {
            await DOCKER_CLIENT.Containers.StopContainerAsync(app.ContainerName, new ContainerStopParameters());
            await DOCKER_CLIENT.Containers.RemoveContainerAsync(app.ContainerName, new ContainerRemoveParameters());
        }
        catch (Exception e)
        {
            //ignored
        }

        try
        {
            var hostConfig = new HostConfig
            {
                RestartPolicy = new RestartPolicy { Name = RestartPolicyKind.UnlessStopped },
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    {
                        $"{app.InternalPort}/tcp",
                        new List<PortBinding> { new() { HostPort = app.ExternalPort.ToString() } }
                    }
                }
            };

            if (app.HostDataPath != null)
            {
                if (!Directory.Exists(app.HostDataPath)) Directory.CreateDirectory(app.HostDataPath);
                await ensureFolderPermissions(app.HostDataPath);

                var containerPath = app.ContainerDataPath ?? "/app/data";
                hostConfig.Binds = new List<string> { $"{app.HostDataPath}:{containerPath}" };
            }

            var createResponse = await DOCKER_CLIENT.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = image,
                Name = app.ContainerName,
                HostConfig = hostConfig
            });

            Log.Debug("[{AppName}] Update complete, starting container {AppContainerName}..", app.Name, app.ContainerName);
            await DOCKER_CLIENT.Containers.StartContainerAsync(createResponse.ID, null);
            await DOCKER_CLIENT.Images.PruneImagesAsync(new ImagesPruneParameters());
            Log.Debug("[{AppName}] Container {AppContainerName} running!", app.Name, app.ContainerName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[{AppName}] Failed to start docker container", app.Name);
        }
    }

    private static async Task ensureFolderPermissions(string path)
    {
        if (OperatingSystem.IsLinux())
        {
            try
            {
                // recursive so it applies to the folder and the .realm file inside
                using var process = Process.Start("chmod", $"-R 777 {path}");
                if (process != null) await process.WaitForExitAsync();

                Log.Debug($"Permissions set to 777 for {path}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to set permissions for {path}: {ex.Message}");
            }
        }
    }

    public static async Task<bool> IsDockerRunningAsync()
    {
        try
        {
            await DOCKER_CLIENT.System.PingAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
