using Spatial4n.Context;
using Spatial4n.Shapes;

namespace Athena.Infrastructure.Lucene.Dto;

public class GeoSearchOption
{
    /// <summary>
    /// 中心点
    /// </summary>
    public virtual IPoint Origin { get; set; }

    /// <summary>
    /// 查询半径（千米）
    /// </summary>
    public virtual double Raidus { get; set; }

    /// <summary>
    /// 最大命中数
    /// </summary>
    public virtual int MaxHits { get; set; }

    private SpatialContext context;
    public GeoSearchOption():this(118.778074408, 32.05723550180)
    {

    }

    public GeoSearchOption(double x,double y,double raidus=1,int maxHits=100)
    {
        context = SpatialContext.Geo;
        Origin = new Point(x, y, context);
        Raidus = raidus;
        MaxHits = maxHits;
    }
}