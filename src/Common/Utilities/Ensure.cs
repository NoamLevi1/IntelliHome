﻿using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace IntelliHome.Common;

public static class Ensure
{
    public static T NotNull<T>([NotNull] T? argument, [CallerArgumentExpression("argument")] string? argumentName = null) =>
        argument != null
            ? argument
            : throw new ArgumentNullException(argumentName);

    public static string NotNullOrWhiteSpace([NotNull] string? argument, [CallerArgumentExpression("argument")] string? argumentName = null) =>
        !argument.IsNullOrWhiteSpace()
            ? argument
            : throw new ArgumentException("value cannot be null or whitespace", argumentName);

    public static IEnumerable<T> NotNullOrEmpty<T>([NotNull] IEnumerable<T>? enumerable, [CallerArgumentExpression("enumerable")] string? argumentName = null) =>
        enumerable?.Any() ?? false
            ? enumerable
            : throw new ArgumentException("value cannot be null or empty", argumentName);
}