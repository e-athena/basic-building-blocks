namespace Athena.Infrastructure.Caching;

/// <summary>
/// 缓存管理接口
/// </summary>
public interface ICacheManager
{
    /// <summary>
    /// 将数据添加至缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="bytes"></param>
    void Set(string key, byte[] bytes);


    /// <summary>
    /// 将数据添加至缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="bytes"></param>
    /// <param name="cancellationToken"></param>
    Task SetAsync(string key, byte[] bytes, CancellationToken cancellationToken = default);

    /// <summary>
    /// 将数据添加至缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="bytes"></param>
    /// <param name="timeSpan"></param>
    void Set(string key, byte[] bytes, TimeSpan timeSpan);


    /// <summary>
    /// 将数据添加至缓存
    /// </summary>
    /// <param name="key"></param>
    /// <param name="bytes"></param>
    /// <param name="timeSpan"></param>
    /// <param name="cancellationToken"></param>
    Task SetAsync(string key, byte[] bytes, TimeSpan timeSpan, CancellationToken cancellationToken = default);

    /// <summary>
    /// 将数据添加至缓存
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="data">数据</param>
    void Set<TItem>(string key, TItem data);

    /// <summary>
    /// 将数据添加至缓存
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="data">数据</param>
    /// <param name="cancellationToken"></param>
    Task SetAsync<TItem>(string key, TItem data, CancellationToken cancellationToken = default);

    /// <summary>
    /// 将数据添加至缓存
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="data">数据</param>
    /// <param name="timeSpan">过期时间</param>
    /// <typeparam name="TItem">泛型类型</typeparam>
    /// <returns></returns>
    void Set<TItem>(string key, TItem data, TimeSpan timeSpan);

    /// <summary>
    /// 将数据添加至缓存
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="data">数据</param>
    /// <param name="timeSpan">过期时间</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TItem">泛型类型</typeparam>
    /// <returns></returns>
    Task SetAsync<TItem>(string key, TItem data, TimeSpan timeSpan, CancellationToken cancellationToken = default);

    /// <summary>
    /// 将数据添加至缓存
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="data">数据</param>
    void SetString(string key, string data);

    /// <summary>
    /// 将数据添加至缓存
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="data">数据</param>
    /// <param name="cancellationToken"></param>
    Task SetStringAsync(string key, string data, CancellationToken cancellationToken = default);

    /// <summary>
    /// 将数据添加至缓存
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="data">数据</param>
    /// <param name="timeSpan">过期时间</param>
    /// <returns></returns>
    void SetString(string key, string data, TimeSpan timeSpan);

    /// <summary>
    /// 将数据添加至缓存
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="data">数据</param>
    /// <param name="timeSpan">过期时间</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SetStringAsync(string key, string data, TimeSpan timeSpan, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取数据，如果数据不存在则创建
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="factory">数据源</param>
    /// <param name="timeSpan">过期时间</param>
    /// <typeparam name="TItem">泛型类型</typeparam>
    /// <returns></returns>
    TItem? GetOrCreate<TItem>(string key, Func<TItem> factory, TimeSpan timeSpan);

    /// <summary>
    /// 读取数据，如果数据不存在则创建
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="factory">数据源</param>
    /// <param name="timeSpan">过期时间</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TItem">泛型类型</typeparam>
    /// <returns></returns>
    Task<TItem?> GetOrCreateAsync<TItem>(string key, Func<Task<TItem>> factory, TimeSpan timeSpan,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取数据，如果数据不存在则创建
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="factory">数据源</param>
    /// <typeparam name="TItem">泛型类型</typeparam>
    /// <returns></returns>
    TItem? GetOrCreate<TItem>(string key, Func<TItem> factory);

    /// <summary>
    /// 读取数据，如果数据不存在则创建
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="factory">数据源</param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TItem">泛型类型</typeparam>
    /// <returns></returns>
    Task<TItem?> GetOrCreateAsync<TItem>(string key, Func<Task<TItem>> factory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="val">结果集</param>
    /// <typeparam name="TItem"></typeparam>
    /// <returns>是否读取成功</returns>
    bool TryGetValue<TItem>(string key, out TItem? val);

    /// <summary>
    /// 获取指定键是否已存在缓存
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns></returns>
    bool IsSet(string key);

    /// <summary>
    /// 获取指定键是否已存在缓存
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> IsSetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取缓存内容
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="key">Key</param>
    /// <returns></returns>
    T? Get<T>(string key);

    /// <summary>
    /// 获取缓存内容
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="key">Key</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);


    /// <summary>
    /// 获取缓存内容
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns></returns>
    string GetString(string key);

    /// <summary>
    /// 获取缓存内容
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> GetStringAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取缓存内容
    /// </summary>
    /// <param name="key">Key</param>
    /// <returns></returns>
    byte[] Get(string key);

    /// <summary>
    /// 获取缓存内容
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<byte[]> GetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 移除一个缓存
    /// </summary>
    /// <param name="key">Key</param>
    void Remove(string key);

    /// <summary>
    /// 移除一个缓存
    /// </summary>
    /// <param name="key">Key</param>
    /// <param name="cancellationToken"></param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 查找所有分区节点中符合给定模式(pattern)的 key
    /// </summary>
    /// <param name="pattern">如：runoob*</param>
    /// <returns></returns>
    IEnumerable<string> Keys(string pattern);

    /// <summary>
    /// 查找所有分区节点中符合给定模式(pattern)的 key
    /// </summary>
    /// <param name="pattern">如：runoob*</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string[]> KeysAsync(string pattern, CancellationToken cancellationToken = default);
    
    
    /// <summary>
    /// 移除所有分区节点中符合给定模式(pattern)的 key
    /// </summary>
    /// <param name="pattern">如：runoob*</param>
    void RemovePattern(string pattern);

    /// <summary>
    /// 移除所有分区节点中符合给定模式(pattern)的 key
    /// </summary>
    /// <param name="pattern">如：runoob*</param>
    /// <param name="cancellationToken"></param>
    Task RemovePatternAsync(string pattern, CancellationToken cancellationToken = default);
}