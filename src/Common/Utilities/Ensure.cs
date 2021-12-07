﻿using System.Runtime.CompilerServices;

namespace IntelliHome.Common;

public static class Ensure
{
    public static T NotNull<T>(T? argument, [CallerArgumentExpression("argument")] string? argumentName = null) =>
        argument != null
            ? argument
            : throw new ArgumentNullException(argumentName);

    public static string NotNullOrWhiteSpace(string? argument, [CallerArgumentExpression("argument")] string? argumentName = null) =>
        !argument.IsNullOrWhiteSpace()
            ? argument
            : throw new ArgumentException("value cannot be null or whitespace", argumentName);

    public static TEnumerable NotNullOrEmpty<TEnumerable, T>(TEnumerable? enumerable, [CallerArgumentExpression("enumerable")] string? argumentName = null)
        where TEnumerable : IEnumerable<T> =>
        enumerable?.Any() ?? false
            ? enumerable
            : throw new ArgumentException("value cannot be null or empty");
}