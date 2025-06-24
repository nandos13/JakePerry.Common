using System;
using System.Collections;
using System.Collections.Generic;

namespace JakePerry.PySharp
{
    // TODO: Unit tests?

    internal readonly struct Int32RangeSequence : IEnumerable, IEnumerable<int>
    {
        internal struct Enumerator : IEnumerator, IEnumerator<int>
        {
            public readonly int stop;
            public readonly int step;

            private int m_current;
            private int m_next;

            public readonly int Current => m_current;

            readonly object IEnumerator.Current => m_current;

            public Enumerator(int start, int stop, int step)
            {
                if (step == 0) throw new ArgumentException("step argument must not be zero", nameof(step));

                this.stop = stop;
                this.step = step;

                // Negative step value is used to iterate backwards
                if (step < 0)
                {
                    m_current = m_next = (start > stop ? start : stop);
                }
                else
                {
                    m_current = m_next = (start < stop ? start : stop);
                }
            }

            public bool MoveNext()
            {
                if (Math.Sign(step) != Math.Sign(stop - m_next))
                {
                    return false;
                }

                m_current = m_next;
                m_next += step;

                return true;
            }

            readonly void IEnumerator.Reset() => throw new NotSupportedException();

            readonly void IDisposable.Dispose() { }
        }

        public readonly int start;
        public readonly int stop;
        public readonly int step;

        public Int32RangeSequence(int start, int stop, int step)
        {
            if (step == 0) throw new ArgumentException("step argument must not be zero", nameof(step));

            this.start = start;
            this.stop = stop;
            this.step = step;
        }

        public Int32RangeSequence(int stop)
            : this(start: 0, stop, step: 1)
        { }

        public Int32RangeSequence(int start, int stop)
            : this(start, stop, step: 1)
        { }

        public readonly Enumerator GetEnumerator()
        {
            return new(start, stop, step);
        }

        IEnumerator IEnumerable.GetEnumerator()
            => this.GetEnumerator();

        IEnumerator<int> IEnumerable<int>.GetEnumerator()
            => this.GetEnumerator();
    }
}
