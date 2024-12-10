namespace JakePerry.Text.Json
{
    internal enum TokenType : byte
    {
        /// <summary>
        /// Represents an undefined token. This generally means that invalid JSON data was encountered.
        /// </summary>
        Undefined,

        /// <summary>
        /// Indicates that the end of file was reached.
        /// </summary>
        EndOfFile,

        /// <summary>
        /// Denotes the beginning of an object '<c>{</c>'.
        /// </summary>
        BeginObject,

        /// <summary>
        /// Denotes the end of an object '<c>}</c>'.
        /// </summary>
        EndObject,

        /// <summary>
        /// Denotes the beginning of an array '<c>[</c>'.
        /// </summary>
        BeginArray,

        /// <summary>
        /// Denotes the end of an array '<c>]</c>'.
        /// </summary>
        EndArray,

        /// <summary>
        /// Denotes a colon character '<c>:</c>'.
        /// </summary>
        Colon,

        /// <summary>
        /// Denotes a comma character '<c>,</c>'.
        /// </summary>
        Comma,

        /// <summary>
        /// Denotes a full string value, beginning and ending with quotation marks '<c>"</c>'.
        /// </summary>
        String,

        /// <summary>
        /// Denotes a number value.
        /// </summary>
        Number,

        /// <summary>
        /// Denotes a literal value '<c>false</c>'.
        /// </summary>
        False,

        /// <summary>
        /// Denotes a literal value '<c>true</c>'.
        /// </summary>
        True,

        /// <summary>
        /// Denotes a literal value '<c>null</c>'.
        /// </summary>
        Null,

        /// <summary>
        /// Denotes the start of an escaped string, beginning with a quotation mark '<c>"</c>'.
        /// Subsequent tokens will be of type <see cref="RawStringData"/> or one of the escaped
        /// token types, until <see cref="EndEscapedString"/> is reached.
        /// </summary>
        BeginEscapedString,

        /// <summary>
        /// Denotes a span of raw unescaped string data found within an escaped string.
        /// </summary>
        RawStringData,

        /// <summary>
        /// Denotes an escaped quotation mark character '<c>\"</c>'.
        /// </summary>
        EscapedQuotationMark,

        /// <summary>
        /// Denotes an escaped reverse solidus character '<c>\\</c>'.
        /// </summary>
        EscapedReverseSolidus,

        /// <summary>
        /// Denotes an escaped solidus character '<c>\/</c>'.
        /// </summary>
        EscapedSolidus,

        /// <summary>
        /// Denotes an escaped backspace character '<c>\b</c>'.
        /// </summary>
        EscapedBackspace,

        /// <summary>
        /// Denotes an escaped formfeed character '<c>\f</c>'.
        /// </summary>
        EscapedFormfeed,

        /// <summary>
        /// Denotes an escaped linefeed character '<c>\n</c>'.
        /// </summary>
        EscapedLinefeed,

        /// <summary>
        /// Denotes an escaped carriage return character '<c>\r</c>'.
        /// </summary>
        EscapedCarriageReturn,

        /// <summary>
        /// Denotes an escaped tab character '<c>\t</c>'.
        /// </summary>
        EscapedHorizontalTab,

        /// <summary>
        /// Denotes an escaped unicode character '<c>\u0000</c>'.
        /// </summary>
        EscapedUnicodeHex,

        /// <summary>
        /// Denotes the end of an escaped string, ended with a quotation mark '<c>"</c>'.
        /// </summary>
        EndEscapedString
    }
}
