using IntelliHome.Cloud.Extensions;
using IntelliHome.Common;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;

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
        builder.Services.
            AddConfigurationManager<CloudConfiguration>().
            AddSingleton<IDatabase,Database>().
            AddSingleton<IForwarderHttpClientFactory, ForwarderHomeApplianceTunneledHttpClientFactory>().
            AddSingleton<ICommunicationRequestSender, CommunicationRequestSender>().
            AddSingleton<ICommunicationManager, CommunicationManager>().
            AddSingleton<IProxyConfigProvider, CustomProxyConfigProvider>();

        builder.Services.AddSignalR().AddNewtonsoftJsonProtocol(options => options.PayloadSerializerSettings.ConfigureCommon());
        builder.Services.AddControllersWithViews().AddNewtonsoftJson(options => options.SerializerSettings.ConfigureCommon());
        builder.Services.AddReverseProxy();

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

        webApplication.MapControllerRouteWithHostNamePrefix(
            "default",
            "{controller=Home}/{action=Index}/{id?}",
            "Portal");
        webApplication.MapReverseProxy();
        webApplication.MapControllers();
        webApplication.MapHub<CommunicationRequestSender>("/Api/CommunicationRequestSender");

        return webApplication;
    }
}