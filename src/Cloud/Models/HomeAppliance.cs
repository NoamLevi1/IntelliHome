namespace IntelliHome.Cloud;

public sealed class HomeAppliance
{
    public Guid Id { get; }
    public string? Name { get; set; }
    public bool IsConnected { get; set; }

    public HomeAppliance(Guid id) =>Id = id;

    public HomeAppliance()
    {
    }
}