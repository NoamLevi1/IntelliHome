using Yarp.ReverseProxy.Configuration;

namespace IntelliHome.Cloud;

public static class Program
{
    public static void Main(string[] args)
    {
        CreateWebApplication(args).Run();
    }

    private static WebApplication CreateWebApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSingleton<IHomeApplianceHttpMessageSender, HomeApplianceHttpMessageSender>();
        builder.Services.AddSignalR();
        builder.Services.AddControllersWithViews();
        builder.Services.AddSingleton<IProxyConfigProvider, CustomProxyConfigProvider>().AddReverseProxy();

        var webApplication = builder.Build();

        if (!webApplication.Environment.IsDevelopment())
        {
            webApplication.
                UseExceptionHandler("/Home/Error").
                UseHsts();
        }

        webApplication.
            UseHttpsRedirection().
            UseStaticFiles().
            UseRouting().
            UseAuthorization();

        webApplication.MapReverseProxy();
        webApplication.
            UseEndpoints(
            endpoints => endpoints.MapHub<HomeApplianceHttpMessageSender>("/Connectionhub"));

        return webApplication;
    }
}