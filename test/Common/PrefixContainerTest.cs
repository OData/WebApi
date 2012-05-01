// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web
{
    [CLSCompliant(false)]
    public class PrefixContainerTests
    {
        [Fact]
        public void Constructor_GuardClauses()
        {
            // Act & assert
            Assert.ThrowsArgumentNull(() => new PrefixContainer(null), "values");
        }

        [Fact]
        public void ContainsPrefix_GuardClauses()
        {
            // Arrange
            var container = new PrefixContainer(new string[0]);

            // Act & assert
            Assert.ThrowsArgumentNull(() => container.ContainsPrefix(null), "prefix");
        }

        [Fact]
        public void ContainsPrefix_EmptyCollectionReturnsFalse()
        {
            // Arrange
            var container = new PrefixContainer(new string[0]);

            // Act & Assert
            Assert.False(container.ContainsPrefix(""));
        }

        [Fact]
        public void ContainsPrefix_ExactMatch()
        {
            // Arrange
            var container = new PrefixContainer(new[] { "Hello" });

            // Act & Assert
            Assert.True(container.ContainsPrefix("Hello"));
        }

        [Fact]
        public void ContainsPrefix_MatchIsCaseInsensitive()
        {
            // Arrange
            var container = new PrefixContainer(new[] { "Hello" });

            // Act & Assert
            Assert.True(container.ContainsPrefix("hello"));
        }

        [Fact]
        public void ContainsPrefix_MatchIsNotSimpleSubstringMatch()
        {
            // Arrange
            var container = new PrefixContainer(new[] { "Hello" });

            // Act & Assert
            Assert.False(container.ContainsPrefix("He"));
        }

        [Fact]
        public void ContainsPrefix_NonEmptyCollectionReturnsTrueIfPrefixIsEmptyString()
        {
            // Arrange
            var container = new PrefixContainer(new[] { "Hello" });

            // Act & Assert
            Assert.True(container.ContainsPrefix(""));
        }

        [Fact]
        public void ContainsPrefix_PrefixBoundaries()
        {
            // Arrange
            var container = new PrefixContainer(new[] { "Hello.There[0]" });

            // Act & Assert
            Assert.True(container.ContainsPrefix("hello"));
            Assert.True(container.ContainsPrefix("hello.there"));
            Assert.True(container.ContainsPrefix("hello.there[0]"));
            Assert.False(container.ContainsPrefix("hello.there.0"));
        }

        [Theory]
        [InlineData("a")]
        [InlineData("a[d]")]
        [InlineData("c.b")]
        [InlineData("c.b.a")]
        public void ContainsPrefix_PositiveTests(string testValue)
        {
            // Arrange
            var container = new PrefixContainer(new[] { "a.b", "c.b.a", "a[d]", "a.c" });

            // Act & Assert
            Assert.True(container.ContainsPrefix(testValue));
        }

        [Theory]
        [InlineData("a.d")]
        [InlineData("b")]
        [InlineData("c.a")]
        [InlineData("c.b.a.a")]
        public void ContainsPrefix_NegativeTests(string testValue)
        {
            // Arrange
            var container = new PrefixContainer(new[] { "a.b", "c.b.a", "a[d]", "a.c" });

            // Act & Assert
            Assert.False(container.ContainsPrefix(testValue));
        }

        [Fact]
        public void GetKeysFromPrefix_DotsNotation()
        {
            // Arrange
            var container = new PrefixContainer(new[] { "foo.bar.baz", "something.other", "foo.baz", "foot.hello", "fo.nothing", "foo" });
            string prefix = "foo";

            // Act
            IDictionary<string, string> result = container.GetKeysFromPrefix(prefix);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.True(result.ContainsKey("bar"));
            Assert.True(result.ContainsKey("baz"));
            Assert.Equal("foo.bar", result["bar"]);
            Assert.Equal("foo.baz", result["baz"]);
        }

        [Fact]
        public void GetKeysFromPrefix_BracketsNotation()
        {
            // Arrange
            var container = new PrefixContainer(new[] { "foo[bar]baz", "something[other]", "foo[baz]", "foot[hello]", "fo[nothing]", "foo" });
            string prefix = "foo";

            // Act
            IDictionary<string, string> result = container.GetKeysFromPrefix(prefix);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.True(result.ContainsKey("bar"));
            Assert.True(result.ContainsKey("baz"));
            Assert.Equal("foo[bar]", result["bar"]);
            Assert.Equal("foo[baz]", result["baz"]);
        }

        [Fact]
        public void GetKeysFromPrefix_MixedDotsAndBrackets()
        {
            // Arrange
            var container = new PrefixContainer(new[] { "foo[bar]baz", "something[other]", "foo.baz", "foot[hello]", "fo[nothing]", "foo" });
            string prefix = "foo";

            // Act
            IDictionary<string, string> result = container.GetKeysFromPrefix(prefix);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.True(result.ContainsKey("bar"));
            Assert.True(result.ContainsKey("baz"));
            Assert.Equal("foo[bar]", result["bar"]);
            Assert.Equal("foo.baz", result["baz"]);
        }

        [Fact]
        public void GetKeysFromPrefix_AllValues()
        {
            // Arrange
            var container = new PrefixContainer(new[] { "foo[bar]baz", "something[other]", "foo.baz", "foot[hello]", "fo[nothing]", "foo" });
            string prefix = "";

            // Act
            IDictionary<string, string> result = container.GetKeysFromPrefix(prefix);

            // Assert
            Assert.Equal(4, result.Count());
            Assert.Equal("foo", result["foo"]);
            Assert.Equal("something", result["something"]);
            Assert.Equal("foot", result["foot"]);
            Assert.Equal("fo", result["fo"]);
        }

        [Fact]
        public void GetKeysFromPrefix_PrefixNotFound()
        {
            // Arrange
            var container = new PrefixContainer(new[] { "foo[bar]", "something[other]", "foo.baz", "foot[hello]", "fo[nothing]", "foo" });
            string prefix = "notfound";

            // Act
            IDictionary<string, string> result = container.GetKeysFromPrefix(prefix);

            // Assert
            Assert.Empty(result);
        }
    }
}
