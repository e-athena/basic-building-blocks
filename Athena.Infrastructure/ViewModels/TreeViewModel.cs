namespace Athena.Infrastructure.ViewModels;

/// <summary>
/// 
/// </summary>
public class TreeViewModel
{
    /// <summary>
    /// Key
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// Title
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Disabled
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// Children
    /// </summary>
    public List<TreeViewModel>? Children { get; set; }
}