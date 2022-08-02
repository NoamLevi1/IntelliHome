using System.ComponentModel;
using IntelliHome.Common;

namespace IntelliHome.Cloud;

public sealed class HomeApplianceCatalogModel
{
    public Guid Id { get; }
    [DisplayName("No.")]
    public int Number { get; }
    [DisplayName("Name")]
    public string Name { get; }

    [DisplayName("Is Connected")]
    public bool IsConnected { get; }

    public Uri Link { get; }

    public HomeApplianceCatalogModel(
        Guid id,
        int number,
        string name,
        bool isConnected,
        Uri link)
    {
        Ensure.NotNullOrWhiteSpace(name);
        Ensure.NotNull(link);

        Id = id;
        Number = number;
        Name = name;
        IsConnected = isConnected;
        Link = link;
    }
}