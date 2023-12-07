namespace Athena.Infrastructure.CSRedis;

/// <summary>
/// Redis缓存适配器
/// </summary>
public class RedisCacheAdapter : ICacheManager
{
    private readonly IDistributedCache _redisService;


    public RedisCacheAdapter(IDistributedCache redisService)
    {
        _redisService = redisService;
    }

    public void Set(string key, byte[] bytes)
    {
        _redisService.Set(key, bytes);
    }

    public async Task SetAsync(string key, byte[] bytes, CancellationToken cancellationToken = default)
    {
        await _redisService.SetAsync(key, bytes, token: cancellationToken);
    }

    public void Set(string key, byte[] bytes, TimeSpan timeSpan)
    {
        _redisService.Set(key, bytes, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = timeSpan
        });
    }

    public async Task SetAsync(string key, byte[] bytes, TimeSpan timeSpan,
        CancellationToken cancellationToken = default)
    {
        await _redisService.SetAsync(key, bytes, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = timeSpan
        }, cancellationToken);
    }


    public void Set<TItem>(string key, TItem data)
    {
        _redisService.Set(key, JsonSerializer.SerializeToUtf8Bytes(data));
    }

    public async Task SetAsync<TItem>(string key, TItem data, CancellationToken cancellationToken = default)
    {
        await _redisService.SetAsync(key, JsonSerializer.SerializeToUtf8Bytes(data), token: cancellationToken);
    }

    public void Set<TItem>(string key, TItem data, TimeSpan timeSpan)
    {
        _redisService.Set(key, JsonSerializer.SerializeToUtf8Bytes(data), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = timeSpan
        });
    }

    public async Task SetAsync<TItem>(string key, TItem data, TimeSpan timeSpan,
        CancellationToken cancellationToken = default)
    {
        await _redisService.SetAsync(key, JsonSerializer.SerializeToUtf8Bytes(data),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = timeSpan
            }, cancellationToken);
    }

    public void Set(string key, string data)
    {
        _redisService.SetString(key, data);
    }

    public void SetString(string key, string data)
    {
        _redisService.SetString(key, data);
    }

    public async Task SetStringAsync(string key, string data, CancellationToken cancellationToken = default)
    {
        await _redisService.SetStringAsync(key, data, token: cancellationToken);
    }

    public void SetString(string key, string data, TimeSpan timeSpan)
    {
        _redisService.SetString(key, data, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = timeSpan
        });
    }

    public async Task SetStringAsync(string key, string data, TimeSpan timeSpan,
        CancellationToken cancellationToken = default)
    {
        await _redisService.SetStringAsync(key, data,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = timeSpan
            }, token: cancellationToken);
    }


    public byte[] Get(string key)
    {
        return _redisService.Get(key) ?? throw new Exception($"Redis缓存中不存在Key为{key}的数据");
    }

    public async Task<byte[]> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        return (await _redisService.GetAsync(key, cancellationToken)) ??
               throw new Exception($"Redis缓存中不存在Key为{key}的数据");
    }

    public TItem? GetOrCreate<TItem>(string key, Func<TItem> factory, TimeSpan timeSpan)
    {
        var result = _redisService.Get(key);
        if (result != null)
        {
            return JsonSerializer.Deserialize<TItem>(result);
        }

        var data = factory();
        _redisService.Set(key, JsonSerializer.SerializeToUtf8Bytes(data), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = timeSpan
        });
        return data;
    }

    public async Task<TItem?> GetOrCreateAsync<TItem>(string key, Func<Task<TItem>> factory, TimeSpan timeSpan,
        CancellationToken cancellationToken = default)
    {
        var result = await _redisService.GetAsync(key, cancellationToken);
        if (result != null)
        {
            return JsonSerializer.Deserialize<TItem>(result);
        }

        var data = await factory();
        await _redisService.SetAsync(key, JsonSerializer.SerializeToUtf8Bytes(data),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = timeSpan
            }, cancellationToken);
        return data;
    }

    public TItem? GetOrCreate<TItem>(string key, Func<TItem> factory)
    {
        var result = _redisService.Get(key);
        if (result != null)
        {
            return JsonSerializer.Deserialize<TItem>(result);
        }

        var data = factory();
        _redisService.Set(key, JsonSerializer.SerializeToUtf8Bytes(data));
        return data;
    }

    public async Task<TItem?> GetOrCreateAsync<TItem>(string key, Func<Task<TItem>> factory,
        CancellationToken cancellationToken = default)
    {
        var result = await _redisService.GetAsync(key, cancellationToken);
        if (result != null)
        {
            return JsonSerializer.Deserialize<TItem>(result);
        }

        var data = await factory();
        await _redisService.SetAsync(key, JsonSerializer.SerializeToUtf8Bytes(data), token: cancellationToken);
        return data;
    }

    public bool TryGetValue<TItem>(string key, out TItem? val)
    {
        var result = _redisService.Get(key);
        val = result != null ? JsonSerializer.Deserialize<TItem>(result) : default;
        return result != null;
    }

    public bool IsSet(string key)
    {
        var result = _redisService.Get(key);
        return result != null;
    }

    public async Task<bool> IsSetAsync(string key, CancellationToken cancellationToken = default)
    {
        var result = await _redisService.GetAsync(key, cancellationToken);
        return result != null;
    }

    public T? Get<T>(string key)
    {
        var result = _redisService.Get(key);
        return result != null ? JsonSerializer.Deserialize<T>(result) : default;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var result = await _redisService.GetAsync(key, cancellationToken);
        return result != null ? JsonSerializer.Deserialize<T>(result) : default;
    }

    public string GetString(string key)
    {
        return _redisService.GetString(key) ?? string.Empty;
    }

    public async Task<string> GetStringAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _redisService.GetStringAsync(key, token: cancellationToken) ?? string.Empty;
    }

    public void Remove(string key)
    {
        _redisService.Remove(key);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _redisService.RemoveAsync(key, cancellationToken);
    }

    public IEnumerable<string> Keys(string pattern)
    {
        var patternKey = RedisHelper.Prefix + pattern;
        return RedisHelper.Keys(patternKey);
    }

    public Task<string[]> KeysAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var patternKey = RedisHelper.Prefix + pattern;
        return RedisHelper.KeysAsync(patternKey);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pattern"></param>
    public void RemovePattern(string pattern)
    {
        var keys = Keys(pattern);
        foreach (var key in keys)
        {
            Remove(key.Replace(RedisHelper.Prefix, ""));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="cancellationToken"></param>
    public async Task RemovePatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var keys = await KeysAsync(pattern, cancellationToken);
        foreach (var key in keys)
        {
            await RemoveAsync(key.Replace(RedisHelper.Prefix, ""), cancellationToken);
        }
    }
}