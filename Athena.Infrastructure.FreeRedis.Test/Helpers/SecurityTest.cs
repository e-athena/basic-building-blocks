using Athena.Infrastructure.Helpers;

namespace Athena.Infrastructure.FreeRedis.Test.Helpers;

public class SecurityTest
{
    [Fact]
    public void Test1()
    {
        for (var i = 0; i < 100; i++)
        {
            // 时间戳1分钟内有效
            var time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60;
            // 随机生成一个字符串+时间戳
            var text = $"{ObjectId.GenerateNewStringId()},{time}";
            var enText = SecurityHelper.Encrypt(text);
            var deText = SecurityHelper.Decrypt(enText);
            Xunit.Assert.Equal(text, deText);
            Xunit.Assert.Equal(56, enText.Length);

            // 判断是否过期
            var time2 = long.Parse(deText!.Split(',')[1]);
            Xunit.Assert.True(time2 > DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }
    }

    [Fact]
    public void Test2()
    {
        const string key = "kasxaxsasdasf";
        for (var i = 0; i < 100; i++)
        {
            var text = ObjectId.GenerateNewStringId();
            var enText = DynamicCodeHelper.GenerateCode(text, key);
            var deText = DynamicCodeHelper.GetBody(enText, key);
            Xunit.Assert.Equal(64, enText.Length);
            Xunit.Assert.Equal(text, deText);
        }
    }
}