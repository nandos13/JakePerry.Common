using System;

namespace JakePerry
{
    /// <summary>
    /// Serves as the base class for exceptions in JakePerry codebase.
    /// </summary>
    public abstract class JpBaseException : Exception
    {
        private const string kUnknownMessage = "An unknown exception occurred.";

        protected JpBaseException() : base(kUnknownMessage) { }
        protected JpBaseException(string message) : base(message) { }
        protected JpBaseException(string message, Exception inner) : base(message, inner) { }
    }
}
