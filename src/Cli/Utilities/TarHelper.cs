using ICSharpCode.SharpZipLib.Tar;

namespace IntelliHome.Cli;

public static class TarHelper
{
    public static Stream CreateFromDirectory(string directory)
    {
        var tarStream = new MemoryStream();

        using var archive = TarArchive.CreateOutputTarArchive(tarStream);
        archive.IsStreamOwner = false;

        foreach (var fileSystemEntry in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
        {
            var entry = TarEntry.CreateEntryFromFile(fileSystemEntry);
            entry.Name = entry.Name.Replace(directory.Replace("\\", "/"), string.Empty).TrimStart('/');
            archive.WriteEntry(entry, false);
        }
        archive.Close();

        tarStream.Seek(0, SeekOrigin.Begin);
        return tarStream;
    }
}