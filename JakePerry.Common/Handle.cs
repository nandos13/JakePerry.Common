using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JakePerry
{
    // TODO: Revise documentation

    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public readonly struct Handle : IDisposable, IStructWithDefaultCheck
    {
        [FieldOffset(0)]
        private readonly MultiVersionHandle m_inner;
        [FieldOffset(8)]
        private readonly int m_version;

        public bool IsDefaultValue => m_inner is null;

        public IDisposable Obj => m_inner?.Obj;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Handle(IDisposable obj)
        {
            Enforce.Argument(obj, nameof(obj)).IsNotNull();

            m_inner = new MultiVersionHandle(obj);
            m_version = 0;
        }

        private Handle(MultiVersionHandle inner, int version)
        {
            m_inner = inner;
            m_version = version;
        }

        /// <summary>
        /// Acquire a copy of this handle. The underlying resource will not be disposed until both the current
        /// handle and the returned handle are disposed.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Handle Acquire()
        {
            if (m_inner is null) return default;

            int v = m_inner.AcquireNewHandle(m_version);

            return new(m_inner, v);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Dispose()
        {
            m_inner?.Release(m_version);
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public readonly struct Handle<T> : IDisposable, IStructWithDefaultCheck
    {
        [FieldOffset(0)]
        private readonly Handle m_typeless;

        [FieldOffset(0)]
        private readonly MultiVersionHandle m_inner;
        [FieldOffset(8)]
        private readonly int m_version;

        public bool IsDefaultValue => m_typeless.IsDefaultValue;

        public IValueHandle<T> Obj => m_inner?.Obj as IValueHandle<T>;

        /// <summary>
        /// The resource held by this handle.
        /// </summary>
        public readonly T Value
        {
            get
            {
                MultiVersionHandle inner = m_inner;
                if (inner is null) return default;

                if (inner.CheckReleased(m_version, false))
                {
                    throw m_inner.CreateReleasedExceptionNoLock(m_version,
                        "Cannot get value from handle; The current handle has been disposed.");
                }

                IValueHandle<T> obj = inner.Obj as IValueHandle<T>;
                return obj.Value;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Handle(IValueHandle<T> implementation)
        {
            Enforce.Argument(implementation, nameof(implementation)).IsNotNull();

            m_typeless = new Handle(implementation);
        }

        private Handle(Handle typeless)
        {
            m_typeless = typeless;
        }

        private Handle(MultiVersionHandle inner, int version)
        {
            m_inner = inner;
            m_version = version;
        }

        /// <summary>
        /// Acquire a copy of this handle. The underlying resource will not be disposed until both the current
        /// handle and the returned handle are disposed.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Handle<T> Acquire()
        {
            // Reimplement Handle.Acquire here to preserve correct debugging stack trace
            if (m_inner is null) return default;

            int v = m_inner.AcquireNewHandle(m_version);

            return new(m_inner, v);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Dispose()
        {
            m_typeless.Dispose();
        }
    }
}
