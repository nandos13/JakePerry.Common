using System;
using System.Diagnostics;

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

        /// <param name="trace">
        /// A informative stack trace indicating where the error occurred.
        /// </param>
        /// <remarks>
        /// This overload is useful for cases where reporting of an error is
        /// deferred rather than immediately following the occurrence of the error.
        /// </remarks>
        /// <inheritdoc cref="LogError(bool, string)"/>
        void LogError(StackTrace trace, string message);

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

        /// <summary>
        /// Log an exception and relevant stacktrace information.
        /// </summary>
        /// <param name="exception">
        /// The exception to be logged.
        /// </param>
        void LogException(Exception exception);
    }
}
