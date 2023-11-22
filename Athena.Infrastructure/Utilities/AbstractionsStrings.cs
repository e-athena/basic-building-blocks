using System.Resources;

namespace Athena.Infrastructure.Utilities;

 /// <summary>
    ///     <para>
    ///		    String resources used in EF exceptions, etc.
    ///     </para>
    ///     <para>
    ///		    These strings are exposed publicly for use by database providers and extensions.
    ///         It is unusual for application code to need these strings.
    ///     </para>
    /// </summary>
    public static class AbstractionsStrings
    {
        private static readonly ResourceManager ResourceManager
            = new("Microsoft.EntityFrameworkCore.Properties.AbstractionsStrings", typeof(AbstractionsStrings).Assembly);

        /// <summary>
        ///     The string argument '{argumentName}' cannot be empty.
        /// </summary>
        public static string ArgumentIsEmpty(object? argumentName)
            => string.Format(
                GetString("ArgumentIsEmpty", nameof(argumentName)),
                argumentName);

        /// <summary>
        ///     The number argument '{argumentName}' cannot be negative number.
        /// </summary>
        public static string ArgumentIsNegativeNumber(object? argumentName)
            => string.Format(
                GetString("ArgumentIsNegativeNumber", nameof(argumentName)),
                argumentName);

        /// <summary>
        ///     The collection argument '{argumentName}' must not contain any empty elements.
        /// </summary>
        public static string CollectionArgumentHasEmptyElements(object? argumentName)
            => string.Format(
                GetString("CollectionArgumentHasEmptyElements", nameof(argumentName)),
                argumentName);

        /// <summary>
        ///     The collection argument '{argumentName}' must contain at least one element.
        /// </summary>
        public static string CollectionArgumentIsEmpty(object? argumentName)
            => string.Format(
                GetString("CollectionArgumentIsEmpty", nameof(argumentName)),
                argumentName);

        private static string GetString(string name, params string[] formatterNames)
        {
            var value = ResourceManager.GetString(name)!;
            for (var i = 0; i < formatterNames.Length; i++)
            {
                value = value.Replace("{" + formatterNames[i] + "}", "{" + i + "}");
            }

            return value;
        }
    }