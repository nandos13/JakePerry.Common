namespace JakePerry.UnitTesting
{
    /// <summary>
    /// Attribute to define dynamic data for a single parameter of a test method
    /// decorated with the <see cref="ParameterCombinationDataAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class ParameterDynamicValuesAttribute : Attribute
    {
        private readonly string m_sourceName;
        private readonly Type? m_declaringType;
        private readonly DynamicDataSourceType m_sourceType;

        public ParameterDynamicValuesAttribute(
            string dynamicDataSourceName,
            DynamicDataSourceType dynamicDataSourceType = DynamicDataSourceType.Property)
        {
            m_sourceName = dynamicDataSourceName;
            m_sourceType = dynamicDataSourceType;
        }

        public ParameterDynamicValuesAttribute(
            string dynamicDataSourceName,
            Type dynamicDataDeclaringType,
            DynamicDataSourceType dynamicDataSourceType = DynamicDataSourceType.Property)
            : this(dynamicDataSourceName, dynamicDataSourceType)
        {
            m_declaringType = dynamicDataDeclaringType;
        }

        public DynamicDataAttribute ToDynamicData()
        {
            return m_declaringType is null
                ? new DynamicDataAttribute(m_sourceName, m_sourceType)
                : new DynamicDataAttribute(m_sourceName, m_declaringType, m_sourceType);
        }
    }
}
