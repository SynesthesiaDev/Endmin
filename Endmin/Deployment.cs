// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
            var createResponse = await docker_client.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = image,
                Name = app.ContainerName,
                HostConfig = new HostConfig
                {
                    RestartPolicy = new RestartPolicy { Name = RestartPolicyKind.UnlessStopped },
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        {
                            $"{app.InternalPort}/tcp",
                            new List<PortBinding> { new() { HostPort = app.ExternalPort.ToString() } }
                        }
                    }
                }
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
}
