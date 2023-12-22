using System.Diagnostics.CodeAnalysis;
using Athena.Infrastructure.Utilities;

namespace Athena.Infrastructure.DataAnnotations.Schema;

/// <summary>
///     Specifies an index to be generated in the database.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class IndexAttribute : Attribute
{
    private bool? _isUnique;
    private string? _name;

    /// <summary>
    ///     Initializes a new instance of the <see cref="T:Microsoft.EntityFrameworkCore.IndexAttribute" /> class.
    /// </summary>
    /// <param name="propertyNames">The properties which constitute the index, in order (there must be at least one).</param>
    public IndexAttribute(params string[] propertyNames)
    {
        Check.NotEmpty(propertyNames, nameof(propertyNames));
        Check.HasNoEmptyElements(propertyNames, nameof(propertyNames));
        PropertyNames = propertyNames.ToList();
    }

    /// <summary>
    ///     The properties which constitute the index, in order.
    /// </summary>
    public IReadOnlyList<string> PropertyNames { get; }

    /// <summary>The name of the index.</summary>
    public string? Name
    {
        get => _name;
        [param: DisallowNull] set => _name = Check.NotNull<string>(value, nameof(value));
    }

    /// <summary>Whether the index is unique.</summary>
    public bool IsUnique
    {
        get => _isUnique.GetValueOrDefault();
        set => _isUnique = value;
    }

    /// <summary>
    ///     Checks whether <see cref="P:Microsoft.EntityFrameworkCore.IndexAttribute.IsUnique" /> has been explicitly set to a value.
    /// </summary>
    public bool IsUniqueHasValue => _isUnique.HasValue;
}