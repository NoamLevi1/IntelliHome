using Docker.DotNet;
using Docker.DotNet.Models;
using IntelliHome.Common;
using IntelliHome.HomeAppliance.CommunicationManager.Data;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IntelliHome.Cli;

public interface IDockerHelper
{
    Task CreateCommunicationManagerContainerAsync(
        bool verbose,
        bool overwrite,
        CancellationToken cancellationToken);

    Task RemoveCommunicationManagerContainerAsync(CancellationToken cancellationToken);

    Task CreateHomeAssistantContainerAsync(
        bool verbose,
        bool overwrite,
        CancellationToken cancellationToken);

    Task RemoveHomeAssistantContainerAsync(CancellationToken cancellationToken);
}

public sealed class DockerHelper : IDockerHelper
{
    private static readonly string _solutionDirectory =
        Path.Combine(
            Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(GeneralInformation.ProcessDirectoryPath)))))!,
            "src");

    private static readonly ContainerInformation _communicationManagerContainerInformation =
        new("homeappliance-communicationmanager", "intellihome/homeappliance-communicationmanager");
    private static readonly ContainerInformation _homeAssistantContainerInformation =
        new("homeassistant", "homeassistant/home-assistant");

    private readonly ILogger<DockerHelper> _logger;
    private readonly IDockerClient _dockerClient;
    private readonly Progress<JSONMessage> _progress;

    public DockerHelper(ILogger<DockerHelper> logger)
    {
        _logger = logger;
        _dockerClient = new DockerClientConfiguration().CreateClient();
        _progress = new Progress<JSONMessage>(
            jsonMessage =>
                _logger.LogInformation(JsonConvert.SerializeObject(jsonMessage)));
    }

    public async Task CreateCommunicationManagerContainerAsync(
        bool verbose,
        bool overwrite,
        CancellationToken cancellationToken)
    {
        await BuildImageAsync();

        var persistentDataPath = Path.Combine(Path.GetTempPath(), "IntelliHome-CommunicationManager");
        var mountSource = CreateMountSource(persistentDataPath, overwrite);

        await CreateContainerAsync(
            _communicationManagerContainerInformation,
            overwrite,
            cancellationToken,
            mounts: new[]
            {
                new Mount
                {
                    Type = "bind",
                    Source =
                        OperatingSystem.IsWindows()
                            ? mountSource.ToLower()
                            : mountSource,
                    Target = $"/app/{ApplicationInformation.PersistantDataDirectoryName}"
                }
            });

        async Task BuildImageAsync()
        {
            _logger.LogInformation($"{nameof(CreateCommunicationManagerContainerAsync)} {nameof(BuildImageAsync)} started");

            await using var fileStream = TarHelper.CreateFromDirectory(_solutionDirectory);
            await _dockerClient.Images.BuildImageFromDockerfileAsync(
                new ImageBuildParameters
                {
                    Dockerfile = "./HomeAppliance.CommunicationManager/Dockerfile",
                    Tags = new[]
                    {
                        _communicationManagerContainerInformation.ImageName
                    }
                },
                fileStream,
                null,
                null,
                verbose
                    ? _progress
                    : new Progress<JSONMessage>(),
                cancellationToken);

            _logger.LogInformation($"{nameof(CreateCommunicationManagerContainerAsync)} {nameof(BuildImageAsync)} finished");
        }
    }

    public async Task RemoveCommunicationManagerContainerAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(RemoveCommunicationManagerContainerAsync)} started");
        var container = await TryGetContainerAsync(_communicationManagerContainerInformation.Name, cancellationToken);

        if (container != null)
        {
            await RemoveContainerAsync(container.ID, cancellationToken);
        }

        _logger.LogInformation($"{nameof(RemoveCommunicationManagerContainerAsync)} finished");
    }

    public async Task CreateHomeAssistantContainerAsync(
        bool verbose,
        bool overwrite,
        CancellationToken cancellationToken)
    {
        await PullImageAsync();

        var configurationDirectoryPath = Path.Combine(Path.GetTempPath(), "HomeAssistantConfig");
        var mountSource = CreateMountSource(configurationDirectoryPath, overwrite);

        await CreateContainerAsync(
            _homeAssistantContainerInformation,
            overwrite,
            cancellationToken,
            new Dictionary<int, int>
            {
                [8123] = 8123
            },
            new[]
            {
                new Mount
                {
                    Type = "bind",
                    Source =
                        OperatingSystem.IsWindows()
                            ? mountSource.ToLower()
                            : mountSource,
                    Target = "/config"
                }
            });

        var configurationPath = Path.Combine(configurationDirectoryPath, "configuration.yaml");
        while (!File.Exists(configurationPath))
        {
            await Task.Delay(200, cancellationToken);
        }

        var isConfigurationChanged = await ConfigureHomeAssistantAsync();

        if (isConfigurationChanged)
        {
            _logger.LogInformation($"{nameof(CreateHomeAssistantContainerAsync)} restarting home assistant started");
            await _dockerClient.Containers.RestartContainerAsync(
                _homeAssistantContainerInformation.Name,
                new ContainerRestartParameters(),
                cancellationToken);
            _logger.LogInformation($"{nameof(CreateHomeAssistantContainerAsync)} restarting home assistant finished");
        }

        async Task PullImageAsync()
        {
            _logger.LogInformation($"{nameof(CreateHomeAssistantContainerAsync)} {nameof(PullImageAsync)} started");
            await _dockerClient.Images.CreateImageAsync(
                new ImagesCreateParameters
                {
                    FromImage = _homeAssistantContainerInformation.ImageName,
                    Tag = "latest"
                },
                new AuthConfig(),
                verbose
                    ? _progress
                    : new Progress<JSONMessage>(),
                cancellationToken);
            _logger.LogInformation($"{nameof(CreateHomeAssistantContainerAsync)} {nameof(PullImageAsync)} finished");
        }

        async Task<bool> ConfigureHomeAssistantAsync()
        {
            _logger.LogInformation($"{nameof(CreateHomeAssistantContainerAsync)} {nameof(ConfigureHomeAssistantAsync)} file started");

            var yamlSerializer = new YamlSerializer();
            var initialConfigurationYaml = await File.ReadAllTextAsync(configurationPath, cancellationToken);
            var configuration = yamlSerializer.Deserialize<Dictionary<string, object>>(initialConfigurationYaml);

            configuration["http"] =
                new Dictionary<string, object>
                {
                    ["use_x_forwarded_for"] = true,
                    ["trusted_proxies"] = new[]
                    {
                        "0.0.0.0/0"
                    }
                };

            var configuredConfigurationYaml = yamlSerializer.Serialize(configuration);
            await File.WriteAllTextAsync(
                configurationPath,
                configuredConfigurationYaml,
                cancellationToken);

            var isChanged = initialConfigurationYaml != configuredConfigurationYaml;

            _logger.LogInformation($"{nameof(CreateHomeAssistantContainerAsync)} {nameof(ConfigureHomeAssistantAsync)} finished [{nameof(isChanged)}={isChanged}]");

            return isChanged;
        }
    }

    public async Task RemoveHomeAssistantContainerAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(RemoveHomeAssistantContainerAsync)} started");
        var container = await TryGetContainerAsync(_homeAssistantContainerInformation.Name, cancellationToken);

        if (container != null)
        {
            await RemoveContainerAsync(container.ID, cancellationToken);
        }

        _logger.LogInformation($"{nameof(RemoveHomeAssistantContainerAsync)} finished");
    }

    private static string CreateMountSource(string sourcePath, bool overwrite)
    {
        if (overwrite && Directory.Exists(sourcePath))
        {
            Directory.Delete(sourcePath, true);
        }

        if (!Directory.Exists(sourcePath))
        {
            Directory.CreateDirectory(sourcePath);
        }

        var mountSource = sourcePath.Replace("\\", "/").Replace(":", string.Empty);
        if (!mountSource.StartsWith("/"))
        {
            mountSource = "/" + mountSource;
        }

        return mountSource;
    }

    private async Task CreateContainerAsync(
        ContainerInformation containerInformation,
        bool overwrite,
        CancellationToken cancellationToken,
        IDictionary<int, int>? portMapping = null,
        IList<Mount>? mounts = null)
    {
        _logger.LogInformation($"{nameof(CreateContainerAsync)} started [{containerInformation} {nameof(overwrite)}={overwrite}]");

        var existingContainer = await TryGetContainerAsync(containerInformation.Name, cancellationToken);
        if (existingContainer != null)
        {
            if (!overwrite)
            {
                throw new Exception($"Container already exist [{containerInformation}]");
            }

            await RemoveContainerAsync(existingContainer.ID, cancellationToken);
        }

        var createContainerResponse =
            await _dockerClient.Containers.CreateContainerAsync(
                new CreateContainerParameters
                {
                    Name = containerInformation.Name,
                    Image = containerInformation.ImageName,
                    ExposedPorts = portMapping?.ToDictionary(
                        keyValuePair => keyValuePair.Key.ToString(),
                        _ => new EmptyStruct()),
                    HostConfig = new HostConfig
                    {
                        RestartPolicy = new RestartPolicy
                        {
                            Name = RestartPolicyKind.UnlessStopped
                        },
                        PortBindings = portMapping?.ToDictionary(
                            keyValuePair => keyValuePair.Key.ToString(),
                            keyValuePair => (IList<PortBinding>) new[]
                            {
                                new PortBinding
                                {
                                    HostPort = keyValuePair.Value.ToString()
                                }
                            }),
                        Mounts = mounts,
                        ExtraHosts = new[]
                        {
                            $"{Guid.Empty}.host.docker.internal:host-gateway"
                        }
                    },
                    Env = new[]
                    {
                        "DOTNET_ENVIRONMENT=Development"
                    }
                },
                cancellationToken);

        await _dockerClient.Containers.StartContainerAsync(
            createContainerResponse.ID,
            new ContainerStartParameters(),
            cancellationToken);

        _logger.LogInformation($"{nameof(CreateContainerAsync)} finished [{containerInformation} {nameof(overwrite)}={overwrite}]");
    }

    private async Task RemoveContainerAsync(string containerId, CancellationToken cancellationToken)
    {
        await _dockerClient.Containers.RemoveContainerAsync(
            containerId,
            new ContainerRemoveParameters
            {
                Force = true,
            },
            cancellationToken);
    }

    private async Task<ContainerListResponse?> TryGetContainerAsync(string name, CancellationToken cancellationToken)
    {
        var containerListResponses =
            await _dockerClient.Containers.ListContainersAsync(
                new ContainersListParameters
                {
                    All = true
                },
                cancellationToken);

        if (containerListResponses == null)
        {
            throw new Exception("Failed retrieving container list");
        }

        return containerListResponses.SingleOrDefault(container => container.Names.Contains($"/{name}"));
    }

    private class ContainerInformation
    {
        public string Name { get; }
        public string ImageName { get; }

        public ContainerInformation(string name, string imageName)
        {
            Ensure.NotNullOrWhiteSpace(name);
            Ensure.NotNullOrWhiteSpace(imageName);

            Name = name;
            ImageName = imageName;
        }

        public override string ToString() => $"{nameof(Name)}={Name} {nameof(ImageName)}={ImageName}";
    }
}