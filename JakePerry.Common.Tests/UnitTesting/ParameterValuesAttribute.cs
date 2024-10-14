namespace JakePerry.UnitTesting
{
    /// <summary>
    /// Attribute to define all possible values for a single parameter of a test method
    /// decorated with the <see cref="ParameterCombinationDataAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class ParameterValuesAttribute : Attribute
    {
        private readonly object?[] m_args;

        public object?[] Arguments => m_args;

        [Obsolete("One or more arguments must be provided.", error: true)]
        public ParameterValuesAttribute() { m_args = Array.Empty<object>(); }

        public ParameterValuesAttribute(params object?[]? args)
        {
            m_args = args ?? Array.Empty<object>();
        }
    }
}
