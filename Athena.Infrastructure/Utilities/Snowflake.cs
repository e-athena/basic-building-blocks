namespace Athena.Infrastructure.Utilities;

/// <summary>
/// 雪花ID
/// </summary>
public class Snowflake
{
    private static long _machineId; //机器ID
    private static long _sequence; //序列号
    private const long InitialTimestamp = 687888001020L; //初始时间戳
    private const long SequenceBits = 13; //序列号识别位数
    private const long MachineIdBits = 10; //机器ID识别位数
    private const long MaxMachineId = -1L ^ -1L << (int) MachineIdBits; //机器ID最大值
    private const long MachineIdShift = SequenceBits; //机器ID偏左移位数
    private const long TimestampLeftShift = SequenceBits + MachineIdBits; //时间戳偏左移位数
    private const long SequenceMask = -1L ^ -1L << (int) SequenceBits; //生成序列号的掩码
    private static long _lastTimestamp = -1L; //最后时间戳

    private static readonly object SyncRoot = new(); //加锁对象
    private static Snowflake? _instance; //单例对象

    private Snowflake(long machineId)
    {
        if (machineId > MaxMachineId)
        {
            throw new Exception($"机器ID不能大于{MaxMachineId}");
        }

        _machineId = machineId;
    }

    /// <summary>
    /// 实例
    /// </summary>
    public static Snowflake Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (SyncRoot)
            {
                _instance ??= new Snowflake(_machineId);
            }

            return _instance;
        }
    }

    /// <summary>
    /// 生成ID
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public long NextId()
    {
        lock (SyncRoot)
        {
            var timestamp = TimeGen();
            if (timestamp < _lastTimestamp)
            {
                throw new Exception("时间戳不能小于上一次生成ID的时间戳");
            }

            if (_lastTimestamp == timestamp)
            {
                //同一毫秒内，序列号自增
                _sequence = (_sequence + 1) & SequenceMask;
                //同一毫秒的序列数已经达到最大
                if (_sequence == 0)
                {
                    timestamp = TilNextMillis(_lastTimestamp);
                }
            }
            else
            {
                //不同毫秒内，序列号置为0
                _sequence = 0L;
            }

            _lastTimestamp = timestamp;
            return ((timestamp - InitialTimestamp) << (int) TimestampLeftShift) |
                   (_machineId << (int) MachineIdShift) |
                   _sequence;
        }
    }

    /// <summary>
    /// 根据ID获取时间戳
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public long GetTimestamp(long id)
    {
        return (id >> (int) TimestampLeftShift) + InitialTimestamp;
    }

    /// <summary>
    /// 根据ID获取时间
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public DateTime GetTime(long id)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(GetTimestamp(id));
    }

    private static long TilNextMillis(long lastTimestamp)
    {
        var timestamp = TimeGen();
        while (timestamp <= lastTimestamp)
        {
            timestamp = TimeGen();
        }

        return timestamp;
    }

    private static long TimeGen()
    {
        return (long) (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    }
}