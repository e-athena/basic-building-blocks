namespace Athena.Infrastructure.Domain.Attributes;

/// <summary>Specifies the data type of the column as a row version.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class RowVersionAttribute : Attribute
{
}