namespace JakePerry
{
    /// <summary>
    /// This interface extends <see cref="IStruct"/> to provide a means to check if an instance
    /// is equal to the <see langword="default"/> struct instance.
    /// <para/>
    /// See <see cref="IStruct"/> documentation below:
    /// <para/>
    /// <inheritdoc cref="IStruct" path="/summary"/>
    /// </summary>
    /// <remarks>
    /// <inheritdoc cref="IStruct" path="/remarks"/>
    /// </remarks>
    public interface IStructWithDefaultCheck : IStruct
    {
        /// <summary>
        /// Indicates whether this instance is equal to the <see langword="default"/> instance.
        /// </summary>
        bool IsDefaultValue { get; }
    }
}
