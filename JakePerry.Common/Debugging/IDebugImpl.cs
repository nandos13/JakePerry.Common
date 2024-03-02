namespace JakePerry.Debugging
{
    /// <summary>
    /// Describes an implementation of common debugging methods.
    /// </summary>
    internal interface IDebugImpl
    {
        /// <summary>
        /// Make an assertion.
        /// </summary>
        /// <param name="condition">
        /// The conditional expression to evaluate. If the condition
        /// is <see langword="true"/>, no failure message is logged.
        /// </param>
        /// <param name="message">
        /// The message to log upon failure
        /// </param>
        void Assert(bool condition, string message);

        /// <summary>
        /// Log an error.
        /// </summary>
        /// <param name="trace">
        /// Flag indicating whether the stack trace is wanted.
        /// </param>
        /// <param name="message">
        /// The message to be logged.
        /// </param>
        void LogError(bool trace, string message);

        /// <summary>
        /// Log an informative message.
        /// </summary>
        /// <param name="trace">
        /// Flag indicating whether the stack trace is wanted.
        /// </param>
        /// <param name="message">
        /// The message to be logged.
        /// </param>
        void LogInfo(bool trace, string message);
    }
}
