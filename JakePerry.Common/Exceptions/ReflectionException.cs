using System;

namespace JakePerry
{
    /// <summary>
    /// An exception resulting from a Reflection failure.
    /// </summary>
    public class ReflectionException : JpBaseException
    {
        public ReflectionException(string message) : base(message) { }
        public ReflectionException(string message, Exception inner) : base(message, inner) { }
    }
}
