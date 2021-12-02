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

        builder.Services.AddControllersWithViews();

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

        webApplication.MapControllerRoute(
            "default",
            "{controller=Home}/{action=Index}/{id?}");

        return webApplication;
    }
}