using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace System;

/// <summary>
///
/// </summary>
public static class StringExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsNullOrWhiteSpace(this string value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty(this string value)
    {
        return string.IsNullOrEmpty(value);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? JsonToObject<T>(this string value)
    {
        return JsonConvert.DeserializeObject<T>(value);
    }
}