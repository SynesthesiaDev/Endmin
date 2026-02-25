// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using CsToml;

namespace Endmin;

[TomlSerializedObject]
public partial class Configuration
{
    [TomlValueOnSerialized]
    public int PollingInterval { get; set; }

    [TomlValueOnSerialized]
    public string GithubToken { get; set; }

    [TomlValueOnSerialized]
    public App[] Apps { get; set; }
}

[TomlSerializedObject]
public partial class App
{
    [TomlValueOnSerialized]
    public string Name { get; set; }

    [TomlValueOnSerialized]
    public string ContainerName { get; set; }

    [TomlValueOnSerialized]
    public string? HostDataPath { get; set; }

    [TomlValueOnSerialized]
    public string? ContainerDataPath { get; set; }

    [TomlValueOnSerialized]
    public string DockerRepository { get; set; }

    [TomlValueOnSerialized]
    public string GithubUser { get; set; }

    [TomlValueOnSerialized]
    public string GithubRepo { get; set; }

    [TomlValueOnSerialized]
    public string GithubBranch { get; set; }

    [TomlValueOnSerialized]
    public int? InternalPort { get; set; }

    [TomlValueOnSerialized]
    public int? ExternalPort { get; set; }
}

[TomlSerializedObject]
public partial class Workflow
{
    [TomlValueOnSerialized]
    public string[] OnPull { get; set; }
}
