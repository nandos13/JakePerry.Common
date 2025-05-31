namespace JakePerry
{
    /// <summary>
    /// An interface which indicates that the implementing object may or may not be in a valid state.
    /// </summary>
    public interface IMightBeValid
    {
        /// <summary>
        /// Indicates whether the object is in a valid state.
        /// </summary>
        bool IsValid { get; }
    }
}
