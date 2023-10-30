namespace Athena.Infrastructure.FreeRedis;

/// <summary>
/// Redis缓存适配器
/// </summary>
public class RedisCacheAdapter : ICacheManager
{
    private readonly RedisClient _redisService;
    private readonly RedisConfig _config;

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    public RedisCacheAdapter(RedisConfig config)
    {
        if (RedisHelper.Instance == null)
        {
            throw new ArgumentNullException(nameof(RedisHelper.Instance),
                "Redis实例未初始化,请调用RedisHelper.Initialization(client)方法初始化");
        }

        _config = config;
        _redisService = RedisHelper.Instance;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="bytes"></param>
    public void Set(string key, byte[] bytes)
    {
        _redisService.Set(key, bytes);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="bytes"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task SetAsync(string key, byte[] bytes, CancellationToken cancellationToken = default)
    {
        _redisService.Set(key, bytes);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="bytes"></param>
    /// <param name="timeSpan"></param>
    public void Set(string key, byte[] bytes, TimeSpan timeSpan)
    {
        _redisService.SetEx(key, (int) timeSpan.TotalSeconds, bytes);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="bytes"></param>
    /// <param name="timeSpan"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task SetAsync(string key, byte[] bytes, TimeSpan timeSpan, CancellationToken cancellationToken = default)
    {
        _redisService.SetEx(key, (int) timeSpan.TotalSeconds, bytes);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <typeparam name="TItem"></typeparam>
    public void Set<TItem>(string key, TItem data)
    {
        _redisService.Set(key, data);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TItem"></typeparam>
    /// <returns></returns>
    public Task SetAsync<TItem>(string key, TItem data, CancellationToken cancellationToken = default)
    {
        _redisService.Set(key, data);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <param name="timeSpan"></param>
    /// <typeparam name="TItem"></typeparam>
    public void Set<TItem>(string key, TItem data, TimeSpan timeSpan)
    {
        _redisService.SetEx(key, (int) timeSpan.TotalSeconds, data);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <param name="timeSpan"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TItem"></typeparam>
    /// <returns></returns>
    public Task SetAsync<TItem>(string key, TItem data, TimeSpan timeSpan,
        CancellationToken cancellationToken = default)
    {
        _redisService.SetEx(key, (int) timeSpan.TotalSeconds, data);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    public void SetString(string key, string data)
    {
        _redisService.Set(key, data);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task SetStringAsync(string key, string data, CancellationToken cancellationToken = default)
    {
        _redisService.Set(key, data);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <param name="timeSpan"></param>
    public void SetString(string key, string data, TimeSpan timeSpan)
    {
        _redisService.SetEx(key, (int) timeSpan.TotalSeconds, data);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <param name="timeSpan"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task SetStringAsync(string key, string data, TimeSpan timeSpan,
        CancellationToken cancellationToken = default)
    {
        _redisService.SetEx(key, (int) timeSpan.TotalSeconds, data);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public byte[] Get(string key)
    {
        return _redisService.Get<byte[]>(key);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<byte[]> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_redisService.Get<byte[]>(key));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <param name="timeSpan"></param>
    /// <typeparam name="TItem"></typeparam>
    /// <returns></returns>
    public TItem GetOrCreate<TItem>(string key, Func<TItem> factory, TimeSpan timeSpan)
    {
        var result = _redisService.Get<TItem>(key);
        if (result != null)
        {
            return result;
        }

        var data = factory();
        Set(key, data, timeSpan);
        return data;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <param name="timeSpan"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TItem"></typeparam>
    /// <returns></returns>
    public async Task<TItem?> GetOrCreateAsync<TItem>(string key, Func<Task<TItem>> factory, TimeSpan timeSpan,
        CancellationToken cancellationToken = default)
    {
        var result = _redisService.Get<TItem>(key);
        if (result != null)
        {
            return result;
        }

        var data = await factory();
        await SetAsync(key, data, timeSpan, cancellationToken);
        return data;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <typeparam name="TItem"></typeparam>
    /// <returns></returns>
    public TItem GetOrCreate<TItem>(string key, Func<TItem> factory)
    {
        var result = _redisService.Get<TItem>(key);
        if (result != null)
        {
            return result;
        }

        var data = factory();
        Set(key, data);
        return data;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TItem"></typeparam>
    /// <returns></returns>
    public async Task<TItem?> GetOrCreateAsync<TItem>(string key, Func<Task<TItem>> factory,
        CancellationToken cancellationToken = default)
    {
        var result = _redisService.Get<TItem>(key);
        if (result != null)
        {
            return result;
        }

        var data = await factory();
        await SetAsync(key, data, cancellationToken);
        return data;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="val"></param>
    /// <typeparam name="TItem"></typeparam>
    /// <returns></returns>
    public bool TryGetValue<TItem>(string key, out TItem? val)
    {
        var result = _redisService.Get<TItem>(key);
        val = result != null ? result : default;
        return result != null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool IsSet(string key)
    {
        var result = _redisService.Get(key);
        return result != null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<bool> IsSetAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_redisService.Exists(key));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T? Get<T>(string key)
    {
        return _redisService.Get<T>(key);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_redisService.Get<T>(key))!;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetString(string key)
    {
        return _redisService.Get<string>(key);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<string> GetStringAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_redisService.Get<string>(key));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    public void Remove(string key)
    {
        _redisService.Expire(key, TimeSpan.Zero);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _redisService.Expire(key, TimeSpan.Zero);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public IEnumerable<string> Keys(string pattern)
    {
        var patternKey = _config.InstanceName + pattern;
        return _redisService.Keys(patternKey);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<string[]> KeysAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var patternKey = _config.InstanceName + pattern;
        return _redisService.KeysAsync(patternKey);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pattern"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void RemovePattern(string pattern)
    {
        var keys = Keys(pattern);
        foreach (var key in keys)
        {
            Remove(key.Replace(_config.InstanceName, ""));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="NotImplementedException"></exception>
    public async Task RemovePatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var keys = await KeysAsync(pattern, cancellationToken);
        foreach (var key in keys)
        {
            await RemoveAsync(key.Replace(_config.InstanceName, ""), cancellationToken);
        }
    }
}