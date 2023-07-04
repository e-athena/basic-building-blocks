namespace Athena.Infrastructure.EventTracking.Messaging.Models;

/// <summary>
/// DecompositionTreeGraphModel
/// </summary>
public class DecompositionTreeGraphModel
{
    /// <summary>
    /// ID
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Value
    /// </summary>
    public DecompositionTreeGraphValue? Value { get; set; }

    /// <summary>
    /// Children
    /// </summary>
    public List<DecompositionTreeGraphModel>? Children { get; set; }
}

/// <summary>
/// DecompositionTreeGraphValue
/// </summary>
public class DecompositionTreeGraphValue
{
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Items
    /// </summary>
    public List<DecompositionTreeGraphValueItem>? Items { get; set; }
}

/// <summary>
/// 
/// </summary>
public class DecompositionTreeGraphValueItem
{
    /// <summary>
    /// Text
    /// </summary>
    public string Text { get; set; } = null!;

    /// <summary>
    /// Value
    /// </summary>
    public string Value { get; set; } = null!;

    /// <summary>
    /// 
    /// </summary>
    public string? Icon { get; set; }
}