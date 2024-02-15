namespace JakePerry
{
    /// <summary>
    /// Extends <see cref="ObjectPool{T}"/> to provide easy-to-use
    /// object pooling for most use cases.
    /// <para/>
    /// Works with any types with a parameterless constructor that
    /// don't require any special teardown logic.
    /// </summary>
    /// <remarks>
    /// If custom activation or teardown logic is required
    /// (ie. to clear memory), see <see cref="DelegatedObjectPool{T}"/>.
    /// </remarks>
    public sealed class EasyObjectPool<T> : ObjectPool<T> where T : class, new()
    {
        protected sealed override T Activate() => new();
    }
}
