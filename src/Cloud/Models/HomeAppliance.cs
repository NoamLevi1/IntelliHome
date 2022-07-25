namespace IntelliHome.Cloud;

public sealed class HomeAppliance
{
    public Guid Id { get; }
    public string? Name { get; set; }
    public bool? IsConnected { get; set; }

    public HomeAppliance(Guid id) =>Id = id;

    public void Aggregate(HomeAppliance other)
    {
        if (Id != other.Id)
        {
            new Exception("ID and other.Id are not equal");
        }
        else
        {
            Name = other.Name;
            if (other.IsConnected != null)
            {
                IsConnected = other.IsConnected;
            }
        }
    }
}