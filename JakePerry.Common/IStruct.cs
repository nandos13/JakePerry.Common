namespace JakePerry
{
    /// <summary>
    /// An interface which indicates the implementing object is a value type.
    /// </summary>
    /// <remarks>
    /// <b>This interface should not be implemented by any reference type</b>; doing so is
    /// considered invalid and may cause unexpected behavior. Additionally, oOther interfaces may
    /// extend this interface if their intention is to only be implemented by value types,
    /// and never by reference types.
    /// <para/>
    /// Generally, methods accepting an <see cref="IStruct"/> instance may benefit from using a
    /// generic parameter with constraints <c>where T : struct, IStruct</c>, to avoid boxing.
    /// </remarks>
    public interface IStruct { }
}
