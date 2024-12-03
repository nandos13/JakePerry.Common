namespace JakePerry.Text.Json
{
    internal enum TokenType : byte
    {
        Undefined,
        EndOfFile,

        BeginObject,
        EndObject,
        BeginArray,
        EndArray,

        Colon,
        Comma,

        String,
        Number,

        False,
        True,
        Null
    }
}
