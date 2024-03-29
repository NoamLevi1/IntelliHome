﻿using ICSharpCode.SharpZipLib.Tar;

namespace IntelliHome.Cli;

public static class TarHelper
{
    public static Stream CreateFromDirectory(string directory)
    {
        var tarStream = new MemoryStream();

        using var archive = TarArchive.CreateOutputTarArchive(tarStream);
        archive.IsStreamOwner = false;

        foreach (var fileSystemEntry in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Where(path => !path.Contains("\\.vs\\")))
        {
            var entry = TarEntry.CreateEntryFromFile(fileSystemEntry);
            entry.Name = entry.Name.Replace(directory.TrimStart('/').Replace("\\", "/"), string.Empty).TrimStart('/');
            archive.WriteEntry(entry, false);
        }
        archive.Close();

        tarStream.Seek(0, SeekOrigin.Begin);
        return tarStream;
    }
}