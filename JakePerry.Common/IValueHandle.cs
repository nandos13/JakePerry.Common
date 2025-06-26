using System;

namespace JakePerry
{
    /// <summary>
    /// Represents a disposable handle to a value of type <typeparamref name="T"/>.
    /// </summary>
    public interface IValueHandle<out T> : IDisposable
    {
        T Value { get; }
    }
}
