using System;
using System.Collections.Generic;

namespace JakePerry
{
    /// <summary>
    /// Describes a single value &amp; provides an event by which to observe
    /// changes to the value.
    /// </summary>
    public sealed class Observable<T>
    {
        private readonly IEqualityComparer<T> m_comparer;
        private T m_value;

        /// <summary>
        /// Invoked when the value changes.
        /// </summary>
        public event Action<T> OnValueChanged;

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public T Value
        {
            get => m_value;
            set
            {
                if (AssignValueUtility.Set(ref m_value, value, m_comparer))
                {
                    OnValueChanged?.Invoke(value);
                }
            }
        }

        public Observable(T value, IEqualityComparer<T> comparer)
        {
            m_comparer = comparer ?? EqualityComparer<T>.Default;
            m_value = value;
        }

        public Observable(T value) : this(value, null) { }

        public Observable() : this(default, null) { }
    }
}
