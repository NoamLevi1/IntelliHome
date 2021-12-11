using IntelliHome.Common;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace IntelliHome.Cli;

public class InstallDependenciesCommand : CommandBase
{
    private static readonly Uri _mongoInstallerUri = new("https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-5.0.4-signed.msi");
    private static readonly Uri _dockerForDesktopUri = new("https://desktop.docker.com/win/main/amd64/Docker%20Desktop%20Installer.exe?utm_source=docker&utm_medium=webreferral&utm_campaign=dd-smartbutton&utm_location=header");

    private readonly HttpClient _httpClient;

    [Option("--download-location=<path>")]
    public string? DownloadLocation { get; set; }

    protected override bool IsRequireAdminPrivileges => true;

    public InstallDependenciesCommand(
        ILoggerFactory loggerFactory,
        CommandLineApplication commandLineApplication,
        HttpClient httpClient)
        : base(loggerFactory, commandLineApplication) =>
        _httpClient = httpClient;

    protected override async Task RunAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation($"{nameof(RunAsync)} started");
        var downloadLocation = DownloadLocation ?? Path.Combine(Path.GetTempPath(), nameof(InstallDependenciesCommand));

        if (Directory.Exists(downloadLocation))
        {
            Directory.Delete(downloadLocation, true);
        }

        Directory.CreateDirectory(downloadLocation);

        await InstallMongoDbAsync(downloadLocation, cancellationToken);
        await InstallDockerAsync(downloadLocation, cancellationToken);
        Logger.LogInformation($"{nameof(RunAsync)} finished");
    }

    private async Task InstallMongoDbAsync(string downloadLocation, CancellationToken cancellationToken)
    {
        Logger.LogInformation($"{nameof(InstallMongoDbAsync)} started");

        if (File.Exists(@"C:\Program Files\MongoDB\Server\5.0\bin\mongo.exe"))
        {
            Logger.LogWarning("Mongo already installed");
            return;
        }

        var installerPath = Path.Combine(downloadLocation, "mongo.msi");

        await DownloadFileAsync(
            _mongoInstallerUri,
            installerPath,
            cancellationToken);

        await RunCommandAsync("msiexec.exe", $"/qb /i {installerPath}", cancellationToken);

        Logger.LogInformation($"{nameof(InstallMongoDbAsync)} finished");
    }

    private async Task InstallDockerAsync(string downloadLocation, CancellationToken cancellationToken)
    {
        Logger.LogInformation($"{nameof(InstallDockerAsync)} started");
        if (IsDockerInstalled())
        {
            Logger.LogWarning("docker already installed");
            return;
        }

        if (!await InstallWsl2Async())
        {
            return;
        }

        var installerPath = Path.Combine(downloadLocation, "DockerInstaller.exe");
        await DownloadFileAsync(_dockerForDesktopUri, installerPath, cancellationToken);
        await RunCommandAsync(installerPath, "install --quiet", cancellationToken);

        Logger.LogInformation($"{nameof(InstallDockerAsync)} finished");

        async Task<bool> InstallWsl2Async()
        {
            Logger.LogInformation($"{nameof(InstallWsl2Async)} started");

            var exitCode = await RunCommandAsync("dism.exe", "/online /enable-feature /featurename:VirtualMachinePlatform /all /norestart", cancellationToken);

            if (exitCode == 3010)
            {
                var shouldRestart = Prompt.GetYesNo("Restart required to finish installing wsl2, would you like to continue?", true);
                if (shouldRestart)
                {
                    await RunCommandAsync("shutdown", "/r /f /t 00", cancellationToken);
                }

                return false;
            }

            await RunCommandAsync("wsl", "--update", cancellationToken);
            await RunCommandAsync("wsl", "--set-default-version 2", cancellationToken);

            Logger.LogInformation($"{nameof(InstallWsl2Async)} finished");

            return true;
        }

        bool IsDockerInstalled()
        {
            var environmentVariable = Environment.GetEnvironmentVariable("PATH");

            if (environmentVariable.IsNullOrWhiteSpace())
            {
                throw new Exception("Could not locate PATH");
            }

            return environmentVariable.
                Split(";").
                Select(directory => Path.Combine(directory, "docker.exe")).
                Any(File.Exists);
        }
    }

    private async Task DownloadFileAsync(
        Uri downloadUri,
        string filePath,
        CancellationToken cancellationToken)
    {
        Logger.LogInformation($"{nameof(DownloadFileAsync)} started [{nameof(downloadUri)}={downloadUri} {nameof(filePath)}={filePath}]");
        await using var webStream = await _httpClient.GetStreamAsync(downloadUri, cancellationToken);
        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await webStream.CopyToAsync(fileStream, cancellationToken);
        Logger.LogInformation($"{nameof(DownloadFileAsync)} finished [{nameof(downloadUri)}={downloadUri} {nameof(filePath)}={filePath}]");
    }

    private async Task<int> RunCommandAsync(string processPath, string arguments, CancellationToken cancellationToken)
    {
        var process = Process.Start(processPath, arguments);
        await using (cancellationToken.Register(() => process.Kill(true)))
        {
            await process.WaitForExitAsync(cancellationToken);
        }

        return process.ExitCode;
    }
}