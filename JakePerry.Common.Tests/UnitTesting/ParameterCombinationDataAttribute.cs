using System.Reflection;

namespace JakePerry.UnitTesting
{
    /// <summary>
    /// Attribute to define in-line data for a test method and generate tests for all
    /// possible combinations of values.
    /// <para/>
    /// Each parameter of a test method decorated with the ParameterCombinationData attribute
    /// must have either:
    /// <para/>
    /// <list type="bullet">
    /// <item>
    /// A <see cref="ParameterDynamicValuesAttribute"/> defining a dynamic data source.
    /// </item>
    /// <item>
    /// A <see cref="ParameterValuesAttribute"/> defining one or more values.
    /// </item>
    /// </list>
    /// Parameters of type <see langword="bool"/> are exempt and will default to
    /// being tested with both <see langword="false"/> &amp; <see langword="true"/>
    /// if no attribute is found.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ParameterCombinationDataAttribute : Attribute, ITestDataSource
    {
        private static object?[] GetParameterValues(ParameterInfo parameter, MethodInfo methodInfo)
        {
            var dynamicAttr = parameter.GetCustomAttribute<ParameterDynamicValuesAttribute>();
            if (dynamicAttr is not null)
            {
                var dynamicData = dynamicAttr.ToDynamicData().GetData(methodInfo).ToArray();
                if (dynamicData.Length == 1)
                {
                    var data = dynamicData[0];
                    if (data.Length > 0) return data;
                }
                else if (dynamicData.Length > 1)
                {
                    if (dynamicData.All(array => array.Length == 1))
                    {
                        return dynamicData.Select(array => array[0]).ToArray();
                    }
                }

                throw new InvalidOperationException(
                    $"{nameof(ParameterDynamicValuesAttribute)} expects a data source that returns an array containing either " +
                    $"exactly one inner array with one or more parameter values, or " +
                    $"multiple inner arrays each containing exactly one parameter value.");
            }

            var attr = parameter.GetCustomAttribute<ParameterValuesAttribute>();
            if (attr is not null)
            {
                return attr.Arguments;
            }

            // Special case: bool parameters default to testing both values if no attribute is found.
            if (parameter.ParameterType == typeof(bool))
            {
                return new object?[2] { false, true };
            }

            // Otherwise, no attribute is an error.
            throw new InvalidOperationException("One or more parameter is not decorated with the DataValues attribute.");
        }

        public IEnumerable<object?[]> GetData(MethodInfo methodInfo)
        {
            // Grab the method's parameters
            var parameters = methodInfo.GetParameters();
            int argCount = parameters.Length;

            // Get all the possible data values & multiply to find the total combination count
            var argumentMap = parameters.Select(p => GetParameterValues(p, methodInfo)).ToArray();
            int total = argumentMap.Select(o => o.Length).Aggregate(1, (x, y) => x * y);

            for (int i = 0; i < total; ++i)
            {
                var args = new object?[argCount];
                var map = argumentMap[argCount - 1];
                args[argCount - 1] = map[i % map.Length];

                int k = map.Length;
                for (int j = argCount - 2; j >= 0; --j)
                {
                    int mod = i / k;

                    map = argumentMap[j];
                    k *= map.Length;

                    args[j] = map[mod % map.Length];
                }

                yield return args;
            }
        }

        public string? GetDisplayName(MethodInfo methodInfo, object?[]? data)
        {
            // TODO: Consider implementing printout replacement logic like the built in DynamicData attribute,
            // but still fall back to this one...
            return UnitTestingUtility.GetTestDisplayName(methodInfo, data);
        }
    }
}
