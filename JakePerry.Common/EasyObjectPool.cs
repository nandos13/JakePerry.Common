namespace JakePerry
{
    /// <summary>
    /// Provides easy-to-use object pooling.
    /// <para>
    /// Extends <see cref="ObjectPool{T}"/> &amp; works with any
    /// class type that has a parameterless constructor.
    /// </para>
    /// </summary>
    public sealed class EasyObjectPool<T> : ObjectPool<T> where T : class, new()
    {
        protected sealed override T Activate() => new();
    }
}
