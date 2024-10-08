using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace System;

/// <summary>
///
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="obj"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T As<T>(this object obj) where T : class
    {
        return (T)obj;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static string ToJson(this object? obj)
    {
        var settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        return JsonConvert.SerializeObject(obj, settings);
    }
}