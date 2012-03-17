using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace System.Web.Http.Internal
{
    public class ValueProviderUtilTest
    {
        [Fact]
        public void CollectionContainsPrefix_EmptyCollectionReturnsFalse()
        {
            // Arrange
            string[] collection = new string[0];

            // Act
            bool retVal = ValueProviderUtil.CollectionContainsPrefix(collection, "");

            // Assert
            Assert.False(retVal);
        }

        [Fact]
        public void CollectionContainsPrefix_ExactMatch()
        {
            // Arrange
            string[] collection = new string[] { "Hello" };

            // Act
            bool retVal = ValueProviderUtil.CollectionContainsPrefix(collection, "Hello");

            // Assert
            Assert.True(retVal);
        }

        [Fact]
        public void CollectionContainsPrefix_MatchIsCaseInsensitive()
        {
            // Arrange
            string[] collection = new string[] { "Hello" };

            // Act
            bool retVal = ValueProviderUtil.CollectionContainsPrefix(collection, "hello");

            // Assert
            Assert.True(retVal);
        }

        [Fact]
        public void CollectionContainsPrefix_MatchIsNotSimpleSubstringMatch()
        {
            // Arrange
            string[] collection = new string[] { "Hello" };

            // Act
            bool retVal = ValueProviderUtil.CollectionContainsPrefix(collection, "He");

            // Assert
            Assert.False(retVal);
        }

        [Fact]
        public void CollectionContainsPrefix_NonEmptyCollectionReturnsTrueIfPrefixIsEmptyString()
        {
            // Arrange
            string[] collection = new string[] { "Hello" };

            // Act
            bool retVal = ValueProviderUtil.CollectionContainsPrefix(collection, "");

            // Assert
            Assert.True(retVal);
        }

        [Fact]
        public void CollectionContainsPrefix_PrefixBoundaries()
        {
            // Arrange
            string[] collection = new string[] { "Hello.There[0]" };

            // Act
            bool retVal1 = ValueProviderUtil.CollectionContainsPrefix(collection, "hello");
            bool retVal2 = ValueProviderUtil.CollectionContainsPrefix(collection, "hello.there");

            // Assert
            Assert.True(retVal1);
            Assert.True(retVal2);
        }

        [Fact]
        public void GetPrefixes()
        {
            // Arrange
            string key = "foo.bar[baz].quux";
            string[] expected = new string[]
            {
                "foo.bar[baz].quux",
                "foo.bar[baz]",
                "foo.bar",
                "foo"
            };

            // Act
            string[] result = ValueProviderUtil.GetPrefixes(key).ToArray();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetKeysFromPrefixWithDotsNotation()
        {
            // Arrange
            IList<string> collection = new List<string>()
            {
                "foo.bar", "something.other", "foo.baz", "foot.hello", "fo.nothing", "foo"
            };
            string prefix = "foo";

            // Act
            IDictionary<string, string> result = ValueProviderUtil.GetKeysFromPrefix(collection, prefix);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.True(result.ContainsKey("bar"));
            Assert.True(result.ContainsKey("baz"));
            Assert.Equal("foo.bar", result["bar"]);
            Assert.Equal("foo.baz", result["baz"]);
        }

        [Fact]
        public void GetKeysFromPrefixWithBracketsNotation()
        {
            // Arrange
            IList<string> collection = new List<string>()
            {
                "foo[bar]", "something[other]", "foo[baz]", "foot[hello]", "fo[nothing]", "foo"
            };
            string prefix = "foo";

            // Act
            IDictionary<string, string> result = ValueProviderUtil.GetKeysFromPrefix(collection, prefix);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.True(result.ContainsKey("bar"));
            Assert.True(result.ContainsKey("baz"));
            Assert.Equal("foo[bar]", result["bar"]);
            Assert.Equal("foo[baz]", result["baz"]);
        }

        [Fact]
        public void GetKeysFromPrefixWithDotsAndBracketsNotation()
        {
            // Arrange
            IList<string> collection = new List<string>()
            {
                "foo[bar]", "something[other]", "foo.baz", "foot[hello]", "fo[nothing]", "foo"
            };
            string prefix = "foo";

            // Act
            IDictionary<string, string> result = ValueProviderUtil.GetKeysFromPrefix(collection, prefix);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.True(result.ContainsKey("bar"));
            Assert.True(result.ContainsKey("baz"));
            Assert.Equal("foo[bar]", result["bar"]);
            Assert.Equal("foo.baz", result["baz"]);
        }

        [Fact]
        public void GetKeysFromPrefixWithPrefixNotFound()
        {
            // Arrange
            IList<string> collection = new List<string>()
            {
                "foo[bar]", "something[other]", "foo.baz", "foot[hello]", "fo[nothing]", "foo"
            };
            string prefix = "notfound";

            // Act
            IDictionary<string, string> result = ValueProviderUtil.GetKeysFromPrefix(collection, prefix);

            // Assert
            Assert.Empty(result);
        }
    }
}
