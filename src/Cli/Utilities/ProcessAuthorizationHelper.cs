using System.Security.Principal;

namespace IntelliHome.Cli;

public static class ProcessAuthorizationHelper
{
    public static bool IsRunningWithAdminPrivileges() =>
        OperatingSystem.IsWindows()
            ? new WindowsPrincipal(WindowsIdentity.GetCurrent()).
                IsInRole(WindowsBuiltInRole.Administrator)
            : throw new PlatformNotSupportedException();
}