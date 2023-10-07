using System.ComponentModel.DataAnnotations.Schema;
using Athena.Infrastructure.Domain;

namespace Athena.Infrastructure.FreeSql.Test.Domain;

[Table("users")]
public class User : FullEntityCore
{
    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 年龄
    /// </summary>
    public int Age { get; set; }
}