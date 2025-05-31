using System;

namespace JakePerry
{
    /// <summary>
    /// An exception that is thrown by the <see cref="Enforce"/> class
    /// when a code expectation is not met.
    /// </summary>
    public sealed class EnforceException : Exception
    {
        public const string DefaultMessage = "Code expectation failed.";

        public EnforceException() : base(DefaultMessage) { }

        public EnforceException(string message)
            : base(message ?? DefaultMessage)
        { }

        public EnforceException(string message, Exception innerException)
            : base(message ?? DefaultMessage, innerException)
        { }
    }
}
