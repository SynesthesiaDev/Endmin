// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Codec;

namespace Endmin;

public record Configuration(int PollingInterval, string GithubToken, List<App> Apps)
{
    public static readonly Codec<Configuration> CODEC = StructCodec.For<Configuration>()
        .Field("PollingInterval", Codecs.INT, c => c.PollingInterval)
        .Field("GithubToken", Codecs.STRING, c => c.GithubToken)
        .Field("Apps", App.CODEC.List(), c => c.Apps)
        .Build((i, s, arg3) => new Configuration(i, s, arg3));
}

public record App(
    string Name,
    string ContainerName,
    string? HostDataPath,
    string? ContainerDataPath,
    string DockerRepository,
    string GithubUser,
    string GithubRepo,
    string GithubBranch,
    int? InternalPort,
    int? ExternalPort
    )
{
    public static readonly Codec<App> CODEC = StructCodec.For<App>()
        .Field("Name", Codecs.STRING, a => a.Name)
        .Field("ContainerName", Codecs.STRING, a => a.ContainerName)
        .Field("HostDataPath", Codecs.STRING.Optional(), a => a.HostDataPath.ToOptional())
        .Field("ContainerDataPath", Codecs.STRING.Optional(), a => a.ContainerDataPath.ToOptional())
        .Field("DockerRepository", Codecs.STRING, a => a.DockerRepository)
        .Field("GithubUser", Codecs.STRING, a => a.GithubUser)
        .Field("GithubRepo", Codecs.STRING, a => a.GithubRepo)
        .Field("GithubBranch", Codecs.STRING, a => a.GithubBranch)
        .Field("InternalPort", Codecs.INT.Optional(), a => a.InternalPort.ToOptional())
        .Field("ExternalPort", Codecs.INT.Optional(), a => a.ExternalPort.ToOptional())
        .Build((s, s1, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10) => new App(s, s1, arg3.Value, arg4.Value, arg5, arg6, arg7, arg8, arg9.Value, arg10.Value));
}
