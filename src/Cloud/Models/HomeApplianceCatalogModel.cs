using System.ComponentModel;
using IntelliHome.Common;

namespace IntelliHome.Cloud;

public sealed class HomeApplianceCatalogModel
{
    [DisplayName("No.")]
    public int Number { get; }
    [DisplayName("Name")]
    public string Name { get; }
    [DisplayName("Is Connected")]
    public bool IsConnected { get; }
    public Uri Link { get; }
    public Guid ID { get; }

    public HomeApplianceCatalogModel(
        int number,
        string name,
        bool isConnected,
        Uri link,
        Guid id)
    {
        Ensure.NotNullOrWhiteSpace(name);
        Ensure.NotNull(link);

        Number = number;
        Name = name;
        IsConnected = isConnected;
        Link = link;
        ID = id;
    }
}