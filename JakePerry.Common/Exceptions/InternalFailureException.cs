using System;

namespace JakePerry
{
    public sealed class InternalFailureException : JpBaseException
    {
        private const string kMessage = "An internal failure occurred";

        public InternalFailureException() : base(kMessage) { }

        public InternalFailureException(string message) : base(message) { }

        public InternalFailureException(string message, Exception inner) : base(message, inner) { }
    }
}
