using IntelliHome.Common;
using IntelliHome.HomeAppliance.CommunicationManager.Data;

namespace IntelliHome.HomeAppliance.CommunicationManager;

public interface IIdGenerator
{
    Task<Guid> GetOrCreateAsync();
}

public sealed class IdGenerator : IIdGenerator
{
    private const string _fileName = "HomeApplianceId.txt";

    private static string FilePath =>
        GeneralInformation.BuildPath(
            ApplicationInformation.PersistantDataDirectoryName,
            _fileName);

    public async Task<Guid> GetOrCreateAsync()
    {
        if (File.Exists(FilePath))
        {
            return Guid.Parse(await File.ReadAllTextAsync(FilePath));
        }

        var id = Guid.NewGuid();
        await File.WriteAllTextAsync(FilePath, id.ToString());

        return id;
    }
}

public sealed class DevelopmentIdGenerator : IIdGenerator
{
    public Task<Guid> GetOrCreateAsync() =>
        Task.FromResult(Guid.Empty);
}