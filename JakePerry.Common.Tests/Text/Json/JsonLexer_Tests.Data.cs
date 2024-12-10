namespace JakePerry.Text.Json.Tests
{
    public partial class JsonLexer_Tests
    {
        private static class Json_TestData
        {
            private static string[]? _validJsonInputs;
            private static TokenType[][]? _validJsonInputTokens;

            private static Exception Uninitialized()
                => new InvalidOperationException("Json test data is not initialized.");

            public static IReadOnlyList<string> ValidJsonInputs => _validJsonInputs ?? throw Uninitialized();

            public static IReadOnlyList<IReadOnlyList<TokenType>> ValidJsonInputTokens => _validJsonInputTokens ?? throw Uninitialized();

            private static (string[], TokenType[][]) InitValidJsonTestData()
            {
                var inputs = new List<string>();
                var tokens = new List<TokenType[]>();

                inputs.Add(string.Empty);
                tokens.Add(new[] { TokenType.EndOfFile });

                inputs.Add(@"true");
                tokens.Add(new[] { TokenType.True, TokenType.EndOfFile });

                inputs.Add(@"false");
                tokens.Add(new[] { TokenType.False, TokenType.EndOfFile });

                inputs.Add(@"null");
                tokens.Add(new[] { TokenType.Null, TokenType.EndOfFile });

                inputs.Add(@"0");
                tokens.Add(new[] { TokenType.Number, TokenType.EndOfFile });

                inputs.Add(@"1");
                tokens.Add(new[] { TokenType.Number, TokenType.EndOfFile });

                inputs.Add(@"-1");
                tokens.Add(new[] { TokenType.Number, TokenType.EndOfFile });

                inputs.Add(@"0.0");
                tokens.Add(new[] { TokenType.Number, TokenType.EndOfFile });

                inputs.Add(@"0.1");
                tokens.Add(new[] { TokenType.Number, TokenType.EndOfFile });

                inputs.Add(@"1.2");
                tokens.Add(new[] { TokenType.Number, TokenType.EndOfFile });

                inputs.Add(@"0e1");
                tokens.Add(new[] { TokenType.Number, TokenType.EndOfFile });

                inputs.Add(@"1.2e+10");
                tokens.Add(new[] { TokenType.Number, TokenType.EndOfFile });

                inputs.Add(@"1.2e-10");
                tokens.Add(new[] { TokenType.Number, TokenType.EndOfFile });

                inputs.Add(@"""\""""");
                tokens.Add(new[] { TokenType.BeginEscapedString, TokenType.EscapedQuotationMark, TokenType.EndEscapedString, TokenType.EndOfFile });

                inputs.Add(@"""\/""");
                tokens.Add(new[] { TokenType.BeginEscapedString, TokenType.EscapedSolidus, TokenType.EndEscapedString, TokenType.EndOfFile });

                inputs.Add(@"""\\""");
                tokens.Add(new[] { TokenType.BeginEscapedString, TokenType.EscapedReverseSolidus, TokenType.EndEscapedString, TokenType.EndOfFile });

                inputs.Add(@"""\b""");
                tokens.Add(new[] { TokenType.BeginEscapedString, TokenType.EscapedBackspace, TokenType.EndEscapedString, TokenType.EndOfFile });

                inputs.Add(@"""\f""");
                tokens.Add(new[] { TokenType.BeginEscapedString, TokenType.EscapedFormfeed, TokenType.EndEscapedString, TokenType.EndOfFile });

                inputs.Add(@"""\n""");
                tokens.Add(new[] { TokenType.BeginEscapedString, TokenType.EscapedLinefeed, TokenType.EndEscapedString, TokenType.EndOfFile });

                inputs.Add(@"""\r""");
                tokens.Add(new[] { TokenType.BeginEscapedString, TokenType.EscapedCarriageReturn, TokenType.EndEscapedString, TokenType.EndOfFile });

                inputs.Add(@"""\t""");
                tokens.Add(new[] { TokenType.BeginEscapedString, TokenType.EscapedHorizontalTab, TokenType.EndEscapedString, TokenType.EndOfFile });

                inputs.Add(@"""\u09af""");
                tokens.Add(new[] { TokenType.BeginEscapedString, TokenType.EscapedUnicodeHex, TokenType.EndEscapedString, TokenType.EndOfFile });

                inputs.Add(@"""\u09AF""");
                tokens.Add(new[] { TokenType.BeginEscapedString, TokenType.EscapedUnicodeHex, TokenType.EndEscapedString, TokenType.EndOfFile });

                inputs.Add(@"""Hello\tWorld\u0021""");
                tokens.Add(new[] { TokenType.BeginEscapedString, TokenType.RawStringData, TokenType.EscapedHorizontalTab, TokenType.RawStringData, TokenType.EscapedUnicodeHex, TokenType.EndEscapedString, TokenType.EndOfFile });

                inputs.Add(@"[ ]");
                tokens.Add(new[] { TokenType.BeginArray, TokenType.EndArray, TokenType.EndOfFile });

                inputs.Add(@"[ null ]");
                tokens.Add(new[] { TokenType.BeginArray, TokenType.Null, TokenType.EndArray, TokenType.EndOfFile });

                inputs.Add(@"{ }");
                tokens.Add(new[] { TokenType.BeginObject, TokenType.EndObject, TokenType.EndOfFile });

                inputs.Add(@"{ ""a_string"": 1 }");
                tokens.Add(new[] { TokenType.BeginObject, TokenType.String, TokenType.Colon, TokenType.Number, TokenType.EndObject, TokenType.EndOfFile });

                inputs.Add(@"{ [ ] }");
                tokens.Add(new[] { TokenType.BeginObject, TokenType.BeginArray, TokenType.EndArray, TokenType.EndObject, TokenType.EndOfFile });

                inputs.Add(@"{ ""a_number"": -1.2e10, ""an_array"": [ ""str_1"", ""str_2"" ], ""an_object"": { ""literal_1"": true, ""literal_2"": null } }");
                tokens.Add(new[]
                {
                    TokenType.BeginObject, TokenType.String, TokenType.Colon, TokenType.Number, TokenType.Comma,
                    TokenType.String, TokenType.Colon, TokenType.BeginArray, TokenType.String, TokenType.Comma, TokenType.String, TokenType.EndArray, TokenType.Comma,
                    TokenType.String, TokenType.Colon, TokenType.BeginObject, TokenType.String, TokenType.Colon, TokenType.True, TokenType.Comma,
                    TokenType.String, TokenType.Colon, TokenType.Null, TokenType.EndObject, TokenType.EndObject, TokenType.EndOfFile
                });

                return (inputs.ToArray(), tokens.ToArray());
            }

            public static void Init()
            {
                (var validInputs, var validInputTokens) = InitValidJsonTestData();

                _validJsonInputs = validInputs.ToArray();
                _validJsonInputTokens = validInputTokens.ToArray();
            }
        }
    }
}
