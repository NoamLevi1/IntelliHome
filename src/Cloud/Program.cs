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
        builder.Services.AddSignalR();
        builder.Services.AddControllersWithViews();
        builder.
            Services.
            AddReverseProxy().
            LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

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
        webApplication.UseRouting();
        webApplication.UseEndpoints(
            endpoints =>
            {
                endpoints.MapHub<ConnectionHub>("/Connectionhub");
            });

        return webApplication;
    }
}