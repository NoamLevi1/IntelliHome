namespace IntelliHome.Cloud;

public interface IDatabase
{
    IDatabaseCollection<HomeAppliance> HomeAppliances { get; }
}

public class Database : IDatabase
{
    public IDatabaseCollection<HomeAppliance> HomeAppliances { get; }

    public Database() => 
        HomeAppliances = new HomeApplianceMockDatabaseCollection();

    private sealed class HomeApplianceMockDatabaseCollection : IDatabaseCollection<HomeAppliance>
    {
        private readonly List<HomeAppliance> _homeAppliances = new();

        public Task InsertAsync(HomeAppliance document)
        {
            _homeAppliances.Add(document);
            return Task.CompletedTask;
        }

        public IAsyncEnumerable<HomeAppliance> Query(Func<HomeAppliance, bool> predicate) => 
            _homeAppliances.Where(predicate).ToAsyncEnumerable();

        public Task UpdateAsync(object id, Action<HomeAppliance> updater)
        {
            updater(_homeAppliances.Single(item => item.Id.Equals(id)));
            return Task.CompletedTask;
        }

        public Task Upsert(object id, HomeAppliance document, Action<HomeAppliance> updater) =>
            _homeAppliances.Exists(item => item.Id.Equals(id))
                ? UpdateAsync(id, updater)
                : InsertAsync(document);
    }
}

public interface IDatabaseCollection<TDocument>
{
    IAsyncEnumerable<TDocument> Query(Func<TDocument, bool> predicate);
    Task UpdateAsync(object id, Action<TDocument> updater);
    Task InsertAsync(TDocument document);
    Task Upsert(object id, TDocument document, Action<TDocument> updater);
}

