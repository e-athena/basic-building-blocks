namespace Athena.Infrastructure.Lucene.Dto;

/// <summary>
/// 索引基本信息
/// </summary>
public class IndexInfo
{
    private string indexPath;

    /// <summary>
    /// 索引路径
    /// </summary>
    public string IndexPath
    {
        get { return indexPath; }
        set { indexPath = value; }
    }

    private int fileNum = 0;

    /// <summary>
    /// 索引文件数量
    /// </summary>
    public int FileNum
    {
        get { return fileNum; }
        set { fileNum = value; }
    }

    private int documentNum = 0;

    /// <summary>
    /// Document文档数量
    /// </summary>
    public int DocumentNum
    {
        get { return documentNum; }
        set { documentNum = value; }
    }

    private int fieldNum = 0;

    /// <summary>
    /// Field索引项数量
    /// </summary>
    public int FieldNum
    {
        get { return fieldNum; }
        set { fieldNum = value; }
    }

    private long termNum = 0;

    /// <summary>
    /// Term词项数量
    /// </summary>
    public long TermNum
    {
        get { return termNum; }
        set { termNum = value; }
    }

    private DateTime updateTime;

    /// <summary>
    /// 最近更新时间
    /// </summary>
    public DateTime UpdateTime
    {
        get { return updateTime; }
        set { updateTime = value; }
    }

    private long version;

    /// <summary>
    /// 版本号
    /// </summary>
    public long Version
    {
        get { return version; }
        set { version = value; }
    }
}