using System.Diagnostics.CodeAnalysis;

namespace Athena.Infrastructure.Utilities;

/// <summary>
///
/// </summary>
[DebuggerStepThrough]
public static class Check
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    /// <param name="parameterName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    [return: NotNull]
    public static T NotNull<T>([AllowNull] [NotNull] T value,
        string parameterName)
    {
        if (value is null)
        {
            NotEmpty(parameterName, nameof(parameterName));

            throw new ArgumentNullException(parameterName);
        }

        return value;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    /// <param name="parameterName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static IReadOnlyList<T> NotEmpty<T>(
        [NotNull] IReadOnlyList<T>? value,
        string parameterName)
    {
        NotNull(value, parameterName);

        if (value.Count == 0)
        {
            NotEmpty(parameterName, nameof(parameterName));

            throw new ArgumentException(AbstractionsStrings.CollectionArgumentIsEmpty(parameterName));
        }

        return value;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    /// <param name="parameterName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static string NotEmpty([NotNull] string? value, string parameterName)
    {
        if (value is null)
        {
            NotEmpty(parameterName, nameof(parameterName));

            throw new ArgumentNullException(parameterName);
        }

        if (value.Trim().Length == 0)
        {
            NotEmpty(parameterName, nameof(parameterName));

            throw new ArgumentException(AbstractionsStrings.ArgumentIsEmpty(parameterName));
        }

        return value;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    /// <param name="parameterName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string? NullButNotEmpty(string? value, string parameterName)
    {
        if (value is not null && value.Length == 0)
        {
            NotEmpty(parameterName, nameof(parameterName));

            throw new ArgumentException(AbstractionsStrings.ArgumentIsEmpty(parameterName));
        }

        return value;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    /// <param name="parameterName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static IReadOnlyList<T> HasNoNulls<T>(
        [NotNull] IReadOnlyList<T>? value,
        string parameterName)
        where T : class
    {
        NotNull(value, parameterName);

        if (value.Any(_ => false))
        {
            NotEmpty(parameterName, nameof(parameterName));

            throw new ArgumentException(parameterName);
        }

        return value;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    /// <param name="parameterName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static IReadOnlyList<string> HasNoEmptyElements(
        [NotNull] IReadOnlyList<string>? value,
        string parameterName)
    {
        NotNull(value, parameterName);

        if (value.Any(string.IsNullOrWhiteSpace))
        {
            NotEmpty(parameterName, nameof(parameterName));

            throw new ArgumentException(AbstractionsStrings.CollectionArgumentHasEmptyElements(parameterName));
        }

        return value;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="message"></param>
    /// <exception cref="Exception"></exception>
    [Conditional("DEBUG")]
    public static void DebugAssert([DoesNotReturnIf(false)] bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception($"Check.DebugAssert failed: {message}");
        }
    }
}