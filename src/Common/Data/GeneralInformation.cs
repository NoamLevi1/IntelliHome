namespace IntelliHome.Common;

public static class GeneralInformation
{
    public static string ProcessDirectoryPath { get; } = AppDomain.CurrentDomain.BaseDirectory;

    public static string BuildPath(params string[] subPaths)
    {
        Ensure.NotNullOrEmpty(subPaths);

        return Path.Combine(new[] {ProcessDirectoryPath}.Concat(subPaths).ToArray());
    }
}