// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Endmin;

public static class Deployment
{
    private static readonly DockerClient docker_client = new DockerClientConfiguration().CreateClient();

    public static async Task DeployContainer(App app, string sha)
    {
        var image = $"{app.DockerRepository}:{sha}";
        Logger.Debug($"[{app.Name}] Updating image {image}..", Logger.Network);

        await docker_client.Images.CreateImageAsync(new ImagesCreateParameters { FromImage = app.DockerRepository, Tag = sha }, null, new Progress<JSONMessage>());

        try
        {
            await docker_client.Containers.StopContainerAsync(app.ContainerName, new ContainerStopParameters());
            await docker_client.Containers.RemoveContainerAsync(app.ContainerName, new ContainerRemoveParameters());
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

            var createResponse = await docker_client.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = image,
                Name = app.ContainerName,
                HostConfig = hostConfig
            });

            Logger.Debug($"[{app.Name}] Update complete, starting container {app.ContainerName}..", Logger.Io);
            await docker_client.Containers.StartContainerAsync(createResponse.ID, null);
            await docker_client.Images.PruneImagesAsync(new ImagesPruneParameters());
            Logger.Debug($"[{app.Name}] Container {app.ContainerName} running!", Logger.Io);
        }
        catch (Exception ex)
        {
            Logger.Error($"[{app.Name}] Failed to start docker container", Logger.Io);
            Logger.Exception(ex, Logger.Io);
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

                Logger.Debug($"Permissions set to 777 for {path}", Logger.Io);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to set permissions for {path}: {ex.Message}");
            }
        }
    }
}
