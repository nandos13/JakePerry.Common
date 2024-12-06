using System.Diagnostics.CodeAnalysis;

namespace JakePerry.Text.Json.Tests
{
    [TestClass]
    public partial class JsonLexer_Tests
    {
        private static IDynamicTestData EnumerateValidJsonInputAndExpectedResults()
        {
            var inputs = Json_TestData.ValidJsonInputs;
            var tokens = Json_TestData.ValidJsonInputTokens;

            for (int i = 0; i < inputs.Count; ++i)
            {
                yield return new object[] { inputs[i], tokens[i].Select(t => (object)t) };
            }
        }

        private static IDynamicTestData JsonAndExpectedTokenPairs => EnumerateValidJsonInputAndExpectedResults();

        [ClassInitialize]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter",
            Justification = "Required for ClassInitialize, tests won't run without the parameter.")]
        public static void InitializeTestData(TestContext c)
        {
            Json_TestData.Init();
        }

        [TestMethod]
        [DynamicData(nameof(JsonAndExpectedTokenPairs))]
        public void Tokenize_FromValidJson_ReturnsCorrectResults(
            string json,
            IEnumerable<object> expected)
        {
            var expectedTokens = expected.Select(o => (TokenType)o);

            var lexedTokens = JsonLexer.TokenizeHeapMemory(json.AsMemory())
                .Select(jtoken => jtoken.type);

            Assert.That.SequenceEqual(expected: expectedTokens, actual: lexedTokens);
        }
    }
}
