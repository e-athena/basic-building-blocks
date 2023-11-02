// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 
/// </summary>
public static class Extensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static T? ConvertTo<T>(this object input)
    {
        return ConvertTo<T>(input.ToString());
    }

    [DebuggerStepThrough]
    private static T? ConvertTo<T>(this string? input)
    {
        try
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T) converter.ConvertFromString(input ?? throw new ArgumentNullException(nameof(input)))!;
        }
        catch (NotSupportedException)
        {
            return default;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="section"></param>
    /// <typeparam name="TModel"></typeparam>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static TModel GetOptions<TModel>(this IConfiguration configuration, string section) where TModel : new()
    {
        var model = new TModel();
        configuration.GetSection(section).Bind(model);
        return model;
    }

    /// <summary>
    /// 读取配置
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="configVariable"></param>
    /// <param name="envVariable"></param>
    /// <param name="callback"></param>
    /// <typeparam name="TModel"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static TModel GetConfig<TModel>(
        this IConfiguration configuration,
        string configVariable,
        string? envVariable = null,
        Action<TModel>? callback = null
    ) where TModel : new()
    {
        var config = configuration.GetOptions<TModel>(configVariable);
        envVariable ??= configVariable.ConvertToEnvKey();
        var env = Environment.GetEnvironmentVariable(envVariable);
        if (!string.IsNullOrEmpty(env))
        {
            config = JsonSerializer.Deserialize<TModel>(env);
        }

        if (config == null)
        {
            throw new ArgumentNullException($"未配置{configVariable}", nameof(config));
        }

        callback?.Invoke(config);

        return config;
    }

    /// <summary>
    /// 读取配置值
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="configVariable"></param>
    /// <param name="envVariable"></param>
    /// <param name="allowNull"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static string? GetConfigValue(
        this IConfiguration configuration,
        string configVariable,
        string envVariable,
        bool allowNull = false
    )
    {
        // 允许空值
        var value = configuration.GetEnvValue<string>(configVariable);
        var env = Environment.GetEnvironmentVariable(envVariable);
        if (!string.IsNullOrEmpty(env))
        {
            value = env;
        }

        if (!allowNull && string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException($"未配置{configVariable}", nameof(value));
        }

        return value;
    }

    /// <summary>
    /// 读取环境变量中的值，如果不存在，则读取配置文件中的值
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="key"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static TResult? GetEnvValue<TResult>(this IConfiguration configuration, string key)
    {
        if (typeof(TResult).IsArray)
        {
            throw new NotSupportedException("不支持数组类型");
        }

        var envKey = key.ConvertToEnvKey();
        var value = Environment.GetEnvironmentVariable(envKey);
        if (value == null)
        {
            return configuration.GetValue<TResult>(key);
        }

        return (TResult) Convert.ChangeType(value, typeof(TResult));
    }

    /// <summary>
    /// 读取环境变量中的值，如果不存在，则读取配置文件中的值
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="key"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static TResult[]? GetEnvValues<TResult>(this IConfiguration configuration, string key)
    {
        if (typeof(TResult).IsArray)
        {
            throw new NotSupportedException("不支持数组类型");
        }

        var envKey = key.ConvertToEnvKey();
        var value = Environment.GetEnvironmentVariable(envKey);
        if (value == null)
        {
            return configuration.GetSection(key).Get<TResult[]>();
        }

        // 按,分割
        var arr = value.Split(",");
        return arr.Select(x => (TResult) Convert.ChangeType(x, typeof(TResult))).ToArray();
    }

    // 转换字符串，如：Module:DbContext:Disabled 转换为MODULE__DB_CONTEXT__DISABLED
    private static string ConvertToEnvKey(this string key)
    {
        // 按:分割字符串，然后转换为大写，最后用__连接
        var arr = key.Split(":");
        var sb = new StringBuilder();
        for (var i = 0; i < arr.Length; i++)
        {
            var item = arr[i];
            sb.Append(item.ToUpperAndAddUnderline());
            if (i != arr.Length - 1)
            {
                sb.Append("__");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="section"></param>
    /// <exception cref="ArgumentNullException"></exception>
    [DebuggerStepThrough]
    public static void CheckOptions(this IConfiguration configuration, string section)
    {
        var val = configuration.GetSection(section).Value;
        if (val == null)
        {
            throw new ArgumentNullException(section);
        }
    }

    /// <summary>
    /// 读取连接字符串
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="configVariable"></param>
    /// <param name="envVariable"></param>
    /// <returns></returns>
    public static string? GetConnectionStringByEnv(
        this IConfiguration configuration,
        string configVariable,
        string envVariable)
    {
        var connectionString = configuration.GetConnectionString(configVariable);
        var env = Environment.GetEnvironmentVariable(envVariable);
        if (!string.IsNullOrEmpty(env))
        {
            connectionString = env;
        }

        return connectionString;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="task"></param>
    /// <param name="millisecondsDelay"></param>
    /// <exception cref="TimeoutException"></exception>
    public static async Task TimeoutAfter(this Task task, int millisecondsDelay)
    {
        var timeoutCancellationTokenSource = new CancellationTokenSource();
        var completedTask =
            await Task.WhenAny(task, Task.Delay(millisecondsDelay, timeoutCancellationTokenSource.Token));
        if (completedTask == task)
        {
            timeoutCancellationTokenSource.Cancel();
        }
        else
        {
            throw new TimeoutException("The operation has timed out.");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="task"></param>
    /// <param name="millisecondsDelay"></param>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    /// <exception cref="TimeoutException"></exception>
    public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, int millisecondsDelay)
    {
        var timeoutCancellationTokenSource = new CancellationTokenSource();
        var completedTask =
            await Task.WhenAny(task, Task.Delay(millisecondsDelay, timeoutCancellationTokenSource.Token));
        if (completedTask != task)
        {
            throw new TimeoutException("The operation has timed out.");
        }

        timeoutCancellationTokenSource.Cancel();
        return task.Result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="condition"></param>
    /// <param name="predicate"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <returns></returns>
    public static IEnumerable<TSource> HasWhere<TSource>(
        this IEnumerable<TSource> source,
        bool condition,
        Func<TSource, bool> predicate)
    {
        return condition ? source.Where(predicate) : source;
    }
}