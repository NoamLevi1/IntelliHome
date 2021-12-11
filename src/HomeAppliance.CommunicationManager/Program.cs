namespace IntelliHome.HomeAppliance.CommunicationManager;

public static class Program
{
    public static void Main(string[] args)
    {
        CreateHost(args).Run();
    }

    private static IHost CreateHost(string[] args) =>
        Host.
            CreateDefaultBuilder(args).
            ConfigureServices(
                services => services.AddHostedService<Worker>()).
            Build();
}