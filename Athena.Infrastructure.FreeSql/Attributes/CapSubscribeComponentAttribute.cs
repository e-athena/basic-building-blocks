namespace Athena.Infrastructure.FreeSql.Attributes;

/// <summary>
/// An attribute to indicate a class is a component.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class CapSubscribeComponentAttribute : Attribute
{
    /// <summary>
    /// The lifetime of the component.
    /// </summary>
    public LifeStyle LifeStyle { get; }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CapSubscribeComponentAttribute() : this(LifeStyle.Transient)
    {
    }

    /// <summary>
    /// Parameterized constructor.
    /// </summary>
    /// <param name="lifeStyle"></param>
    public CapSubscribeComponentAttribute(LifeStyle lifeStyle)
    {
        LifeStyle = lifeStyle;
    }
}

/// <summary>An enum to description the lifetime of a component.
/// </summary>
public enum LifeStyle
{
    /// <summary>
    /// Represents a component is a transient component.
    /// </summary>
    Transient,

    /// <summary>
    /// Represents a component is a singleton component.
    /// </summary>
    Singleton,

    /// <summary>
    /// Represents a component is a scoped component.
    /// </summary>
    Scoped
}