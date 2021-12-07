using System.Diagnostics.CodeAnalysis;

namespace IntelliHome.Common;

public static class StringExtension
{
    public static bool IsNullOrWhiteSpace([NotNullWhen(false)] this string? str) =>
        string.IsNullOrWhiteSpace(str);
}