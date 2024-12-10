using System.Collections;
using System.Reflection;

namespace JakePerry.UnitTesting
{
    public static class UnitTestingUtility
    {
        private static Func<object?, string?>? _stringifySelector;

        private static Func<object?, string?> StringifySelector
            => _stringifySelector ??= StringifyArgument;

        /// <summary>
        /// Get a nice readable string representation of an object.
        /// </summary>
        public static string? StringifyArgument(object? o)
        {
            if (o is null) return "null";
            if (o is string s) return $"\"{s}\"";
            if (o is char c) return $"\'{c}\'";

            if (o is IEnumerable collection)
            {
                return DebugUtil.StringifyCollection(
                    collection.Cast<object>().Select(StringifySelector),
                    printCollectionType: false,
                    printMetadata: false);
            }

            return o.ToString();
        }

        /// <summary>
        /// Get a readable string representation of a test method's name and arguments.
        /// <para/>
        /// Intended as a global replacement for test display names
        /// by using <see cref="DynamicDataAttribute.DynamicDataDisplayName"/>, etc.
        /// </summary>
        public static string GetTestDisplayName(MethodInfo methodInfo, object?[]? data)
        {
            if (data is not null)
            {
                var argsString = string.Join(", ", data.Select(StringifySelector));
                return $"{methodInfo.Name} ({argsString})";
            }

            return methodInfo.Name;
        }
    }
}
