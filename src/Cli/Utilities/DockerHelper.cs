using Docker.DotNet;
using Docker.DotNet.Models;
using IntelliHome.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IntelliHome.Cli.Utilities;

public sealed class DockerHelper
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
        await CreateContainerAsync(
            _communicationManagerContainerInformation,
            overwrite,
            cancellationToken);

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
                        _communicationManagerContainerInformation.Name
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

        var configPath = Path.Combine(Path.GetTempPath(), "HomeAssistantConfig");
        if (!Directory.Exists(configPath))
        {
            Directory.CreateDirectory(configPath);
        }

        var mountSource = configPath.Replace("\\", "/").Replace(":", string.Empty);
        if (!mountSource.StartsWith("/"))
        {
            mountSource = "/" + mountSource;
        }

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
                    Source = mountSource.ToLower(),
                    Target = "/config"
                }
            });

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
                        Mounts = mounts
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