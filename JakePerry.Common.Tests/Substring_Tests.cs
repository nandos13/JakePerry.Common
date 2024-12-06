namespace JakePerry.Tests
{
    // TODO: Simplify test data cases that use "abc" where "ab" will do fine
    // TODO: The constructor "does not throw" cases probably should be extended to compare contents to expected string
    //       ie. "hello world" starting at 6 should be equal to "world".
    // TODO: Add test cases for more methods like AsSpan, CompareTo, Starts/EndsWith, Equals, more

    [TestClass]
    public class Substring_Tests
    {
        // Input strings for 'Split' method tests
        private static IDynamicTestData StringSplitInputData
            => new[] { new object[] { "", " ", ",", " ,", ", ", "a", "a,b", ",a,b", "a,b,", "a,,b", "a b", " a, b " } };

        // Separator strings/char spans for 'Split' method tests
        private static IDynamicTestData StringSplitSeparatorCharStrings
            => new[] { new object[] { ",", "a", "b", ",a", ",b", "ab", " ,", " a" } };

        [TestMethod]
        public void Cast_FromNullString_Throws()
        {
            string? nullString = null;

            Func<object?> action = () => (Substring)nullString;

            Assert.ThrowsException<ArgumentNullException>(action);
        }

        [TestMethod]
        public void Construct_FromNullString_Throws()
        {
            string? nullString = null;

            Func<object?> action = () => new Substring(nullString);

            Assert.ThrowsException<ArgumentNullException>(action);
        }

        [TestMethod]
        public void Cast_FromEmptyString_DoesNotThrow()
        {
            var emptyString = string.Empty;

            _ = (Substring)emptyString;
        }

        [TestMethod]
        public void Construct_FromEmptyString_DoesNotThrow()
        {
            var emptyString = string.Empty;

            _ = new Substring(emptyString);
        }

        [TestMethod]
        public void Construct_StartIndexEqualsStringLength_DoesNotThrow()
        {
            var value = "a";

            _ = new Substring(value, value.Length);
        }

        [TestMethod]
        [DataRow("a", 0, 0)]
        [DataRow("a", 0, 1)]
        [DataRow("abc", 0, 3)]
        [DataRow("abc", 2, 1)]
        public void Construct_ValidArguments_DoesNotThrow(string value, int startIndex, int length)
        {
            _ = new Substring(value, startIndex, length);
        }

        [TestMethod]
        [DataRow("a", 0, 0, 0, 0)]
        [DataRow("a", 0, 1, 0, 1)]
        [DataRow("a", 1, 0, 0, 0)]
        [DataRow("ab", 0, 2, 0, 2)]
        [DataRow("ab", 0, 2, 1, 1)]
        [DataRow("ab", 0, 2, 2, 0)]
        public void ConstructFromOther_ValidOffset_GetsExpectedLength(string value, int startIndex, int length, int secondStartIndex, int expectedLength)
        {
            Substring sourceSubstring;
            try
            {
                sourceSubstring = new Substring(value, startIndex, length);
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed to create source substring. " + ex);
                return;
            }

            var substring = new Substring(sourceSubstring, secondStartIndex);

            Assert.AreEqual(expectedLength, substring.Length);
        }

        [TestMethod]
        [DataRow("a", 0, 0, 0, 0)]
        [DataRow("a", 0, 1, 0, 0)]
        [DataRow("a", 0, 1, 0, 1)]
        [DataRow("abc", 0, 3, 0, 3)]
        [DataRow("abc", 0, 3, 2, 1)]
        [DataRow("abc", 2, 1, 0, 1)]
        public void ConstructFromOther_ValidArguments_DoesNotThrow(string value, int startIndex, int length, int secondStartIndex, int secondLength)
        {
            Substring sourceSubstring;
            try
            {
                sourceSubstring = new Substring(value, startIndex, length);
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed to create source substring. " + ex);
                return;
            }

            _ = new Substring(sourceSubstring, secondStartIndex, secondLength);
        }

        [TestMethod]
        [DataRow("a", -1, 0)]
        [DataRow("a", 0, -1)]
        [DataRow("a", 0, 2)]
        [DataRow("a", 1, 1)]
        [DataRow("a", 2, 0)]
        [DataRow("abc", 1, 3)]
        public void Construct_InvalidArguments_Throws(string value, int startIndex, int length)
        {
            Func<object?> action = () => new Substring(value, startIndex, length);

            Assert.ThrowsException<ArgumentOutOfRangeException>(action);
        }

        [TestMethod]
        [DataRow("a", 0, 0, -1, 0)]
        [DataRow("a", 0, 0, 0, -1)]
        [DataRow("a", 0, 0, 0, 1)]
        [DataRow("a", 0, 0, 1, 0)]
        [DataRow("a", 0, 1, 0, 2)]
        [DataRow("a", 0, 1, 1, 1)]
        [DataRow("abc", 0, 3, 0, 4)]
        [DataRow("abc", 2, 1, 0, 2)]
        [DataRow("abc", 2, 1, 1, 1)]
        [DataRow("abc", 2, 1, 2, 0)]
        public void ConstructFromOther_InvalidArguments_Throws(string value, int startIndex, int length, int secondStartIndex, int secondLength)
        {
            Substring sourceSubstring;
            try
            {
                sourceSubstring = new Substring(value, startIndex, length);
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed to create source substring. " + ex);
                return;
            }

            Func<object?> action = () => new Substring(sourceSubstring, secondStartIndex, secondLength);

            Assert.ThrowsException<ArgumentOutOfRangeException>(action);
        }

        [TestMethod]
        public void Constructor_ZeroLength_DoesNotKeepSourceString()
        {
            Assert.AreEqual(string.Empty, new Substring("a", 0, 0).SourceString);
        }

        [TestMethod]
        [DataRow("a", 0, 1, 0, 'a')]
        [DataRow("ab", 0, 1, 0, 'a')]
        [DataRow("ab", 1, 1, 0, 'b')]
        [DataRow("ab", 0, 2, 1, 'b')]
        public void Indexer_ValidIndex_ReturnsCorrectResults(string value, int startIndex, int length, int accessIndex, char expected)
        {
            var substring = new Substring(value, startIndex, length);

            char actual = substring[accessIndex];

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DataRow("", 0, 0, -1)]
        [DataRow("", 0, 0, 0)]
        [DataRow("a", 0, 1, 1)]
        [DataRow("ab", 0, 1, 1)]
        public void Indexer_InvalidIndex_Throws(string value, int startIndex, int length, int accessIndex)
        {
            var substring = new Substring(value, startIndex, length);

            Func<object?> action = () => substring[accessIndex];

            Assert.ThrowsException<ArgumentOutOfRangeException>(action);
        }

        [TestMethod]
        [ParameterCombinationData]
        public void Split_Whitespace_MatchesStringImpl(
            [ParameterDynamicValues(nameof(StringSplitInputData))]
            string str,
            [ParameterValues(null, 1, 2, 3)]
            int? count,
            [ParameterValues(StringSplitOptions.None, StringSplitOptions.RemoveEmptyEntries)]
            StringSplitOptions options)
        {
            var separators = Array.Empty<char>();

            var splits1 = new Substring(str).Split(separators, count, options);
            var splits2 = count.HasValue ? str.Split(separators, count.Value, options) : str.Split(separators, options);

            Assert.That.SequenceEqual(expected: splits2, actual: splits1.Select(s => s.ToString()));
        }

        [TestMethod]
        [ParameterCombinationData]
        public void Split_CharSeparator_MatchesStringImpl(
            [ParameterValues(',')]
            char separator,
            [ParameterDynamicValues(nameof(StringSplitInputData))]
            string str,
            [ParameterValues(null, 1, 2, 3)]
            int? count,
            [ParameterValues(StringSplitOptions.None, StringSplitOptions.RemoveEmptyEntries)]
            StringSplitOptions options)
        {
            var splits1 = new Substring(str).Split(separator, count, options);
            var splits2 = count.HasValue ? str.Split(separator, count.Value, options) : str.Split(separator, options);

            Assert.That.SequenceEqual(expected: splits2, actual: splits1.Select(s => s.ToString()));
        }

        [TestMethod]
        [ParameterCombinationData]
        public void Split_CharSpanSeparator_MatchesStringImpl(
            [ParameterDynamicValues(nameof(StringSplitSeparatorCharStrings))]
            string separatorChars,
            [ParameterDynamicValues(nameof(StringSplitInputData))]
            string str,
            [ParameterValues(null, 1, 2, 3)]
            int? count,
            [ParameterValues(StringSplitOptions.None, StringSplitOptions.RemoveEmptyEntries)]
            StringSplitOptions options)
        {
            var separators = separatorChars.ToCharArray();

            var splits1 = new Substring(str).Split(separators.AsSpan(), count, options);
            var splits2 = count.HasValue ? str.Split(separators, count.Value, options) : str.Split(separators, options);

            Assert.That.SequenceEqual(expected: splits2, actual: splits1.Select(s => s.ToString()));
        }

        [TestMethod]
        [ParameterCombinationData]
        public void Split_StringSeparator_MatchesStringImpl(
            [ParameterDynamicValues(nameof(StringSplitSeparatorCharStrings))]
            string separator,
            [ParameterDynamicValues(nameof(StringSplitInputData))]
            string str,
            [ParameterValues(null, 1, 2, 3)]
            int? count,
            [ParameterValues(StringSplitOptions.None, StringSplitOptions.RemoveEmptyEntries)]
            StringSplitOptions options)
        {
            var splits1 = new Substring(str).Split(separator, count, options);
            var splits2 = count.HasValue ? str.Split(separator, count.Value, options) : str.Split(separator, options);

            Assert.That.SequenceEqual(expected: splits2, actual: splits1.Select(s => s.ToString()));
        }

        [TestMethod]
        [ParameterCombinationData]
        public void Split_StringSpanSeparators_MatchesStringImpl(
            [ParameterValues(new[] { "," }, new[] { ",", "a" }, new[] { "a", "b" })]
            string[] separators,
            [ParameterDynamicValues(nameof(StringSplitInputData))]
            string str,
            [ParameterValues(null, 1, 2, 3)]
            int? count,
            [ParameterValues(StringSplitOptions.None, StringSplitOptions.RemoveEmptyEntries)]
            StringSplitOptions options)
        {
            var splits1 = new Substring(str).Split(separators, count, options);
            var splits2 = count.HasValue ? str.Split(separators, count.Value, options) : str.Split(separators, options);

            Assert.That.SequenceEqual(expected: splits2, actual: splits1.Select(s => s.ToString()));
        }
    }
}
