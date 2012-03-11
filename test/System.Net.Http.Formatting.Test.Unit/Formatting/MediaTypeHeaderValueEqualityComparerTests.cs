using System.Net.Http.Formatting.DataSets;
using System.Net.Http.Headers;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class MediaTypeHeaderValueEqualityComparerTests
    {

        [Fact]
        [Trait("Description", "MediaTypeHeaderValueEqualityComparer is internal, concrete, and not sealed.")]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(typeof(MediaTypeHeaderValueEqualityComparer), TypeAssert.TypeProperties.IsClass);
        }

        [Fact]
        [Trait("Description", "EqualityComparer returns same MediaTypeHeadeValueEqualityComparer instance each time.")]
        public void EqualityComparerReturnsMediaTypeHeadeValueEqualityComparer()
        {
            MediaTypeHeaderValueEqualityComparer comparer1 = MediaTypeHeaderValueEqualityComparer.EqualityComparer;
            MediaTypeHeaderValueEqualityComparer comparer2 = MediaTypeHeaderValueEqualityComparer.EqualityComparer;

            Assert.NotNull(comparer1);
            Assert.Same(comparer1, comparer2);
        }

        [Fact]
        [Trait("Description", "GetHashCode(MediaTypeHeaderValue) returns the same hash code for media types that differe only be case.")]
        public void GetHashCodeReturnsSameHashCodeRegardlessOfCase()
        {
            MediaTypeHeaderValueEqualityComparer comparer = MediaTypeHeaderValueEqualityComparer.EqualityComparer;

            MediaTypeHeaderValue mediaType1 = new MediaTypeHeaderValue("text/xml");
            MediaTypeHeaderValue mediaType2 = new MediaTypeHeaderValue("TEXT/xml");
            Assert.Equal(comparer.GetHashCode(mediaType1), comparer.GetHashCode(mediaType2));

            mediaType1 = new MediaTypeHeaderValue("text/*");
            mediaType2 = new MediaTypeHeaderValue("TEXT/*");
            Assert.Equal(comparer.GetHashCode(mediaType1), comparer.GetHashCode(mediaType2));

            mediaType1 = new MediaTypeHeaderValue("*/*");
            mediaType2 = new MediaTypeHeaderValue("*/*");
            Assert.Equal(comparer.GetHashCode(mediaType1), comparer.GetHashCode(mediaType2));
        }


        [Fact]
        [Trait("Description", "GetHashCode(MediaTypeHeaderValue) returns different hash codes if the media types are different.")]
        public void GetHashCodeReturnsDifferentHashCodeForDifferentMediaType()
        {
            MediaTypeHeaderValueEqualityComparer comparer = MediaTypeHeaderValueEqualityComparer.EqualityComparer;

            MediaTypeHeaderValue mediaType1 = new MediaTypeHeaderValue("text/*");
            MediaTypeHeaderValue mediaType2 = new MediaTypeHeaderValue("TEXT/xml");
            Assert.NotEqual(comparer.GetHashCode(mediaType1), comparer.GetHashCode(mediaType2));

            mediaType1 = new MediaTypeHeaderValue("application/*");
            mediaType2 = new MediaTypeHeaderValue("TEXT/*");
            Assert.NotEqual(comparer.GetHashCode(mediaType1), comparer.GetHashCode(mediaType2));

            mediaType1 = new MediaTypeHeaderValue("application/*");
            mediaType2 = new MediaTypeHeaderValue("*/*");
            Assert.NotEqual(comparer.GetHashCode(mediaType1), comparer.GetHashCode(mediaType2));
        }


        [Fact]
        [Trait("Description", "Equals(MediaTypeHeaderValue, MediaTypeHeaderValue) returns true if media type 1 is a subset of 2.")]
        public void EqualsReturnsTrueIfMediaType1IsSubset()
        {
            string[] parameters = new string[]
            {
                ";name=value",
                ";q=1.0",
                ";version=1",
            };

            MediaTypeHeaderValueEqualityComparer comparer = MediaTypeHeaderValueEqualityComparer.EqualityComparer;

            MediaTypeHeaderValue mediaType1 = new MediaTypeHeaderValue("text/*");
            mediaType1.CharSet = "someCharset";
            MediaTypeHeaderValue mediaType2 = new MediaTypeHeaderValue("TEXT/*");
            mediaType2.CharSet = "SOMECHARSET";
            Assert.Equal(mediaType1, mediaType2, comparer);

            mediaType1 = new MediaTypeHeaderValue("application/*");
            mediaType1.CharSet = "";
            mediaType2 = new MediaTypeHeaderValue("application/*");
            mediaType2.CharSet = null;
            Assert.Equal(mediaType1, mediaType2, comparer);

            foreach (string parameter in parameters)
            {
                mediaType1 = new MediaTypeHeaderValue("text/xml");
                mediaType2 = MediaTypeHeaderValue.Parse("TEXT/xml" + parameter);
                Assert.Equal(mediaType1, mediaType2, comparer);

                mediaType1 = new MediaTypeHeaderValue("text/*");
                mediaType2 = MediaTypeHeaderValue.Parse("TEXT/*" + parameter);
                Assert.Equal(mediaType1, mediaType2, comparer);

                mediaType1 = new MediaTypeHeaderValue("*/*");
                mediaType2 = MediaTypeHeaderValue.Parse("*/*" + parameter);
                Assert.Equal(mediaType1, mediaType2, comparer);
            }
        }

        [Fact]
        [Trait("Description", "Equals(MediaTypeHeaderValue, MediaTypeHeaderValue) returns false if media type 1 is a superset of 2.")]
        public void EqualsReturnsFalseIfMediaType1IsSuperset()
        {
            string[] parameters = new string[]
            {
                ";name=value",
                ";q=1.0",
                ";version=1",
            };

            MediaTypeHeaderValueEqualityComparer comparer = MediaTypeHeaderValueEqualityComparer.EqualityComparer;

            foreach (string parameter in parameters)
            {
                MediaTypeHeaderValue mediaType1 = MediaTypeHeaderValue.Parse("text/xml" + parameter);
                MediaTypeHeaderValue mediaType2 = new MediaTypeHeaderValue("TEXT/xml");
                Assert.NotEqual(mediaType1, mediaType2, comparer);

                mediaType1 = MediaTypeHeaderValue.Parse("text/*" + parameter);
                mediaType2 = new MediaTypeHeaderValue("TEXT/*");
                Assert.NotEqual(mediaType1, mediaType2, comparer);

                mediaType1 = MediaTypeHeaderValue.Parse("*/*" + parameter);
                mediaType2 = new MediaTypeHeaderValue("*/*");
                Assert.NotEqual(mediaType1, mediaType2, comparer);
            }
        }

        [Fact]
        [Trait("Description", "Equals(MediaTypeHeaderValue, MediaTypeHeaderValue) returns true if media types and charsets differ only by case.")]
        public void Equals1ReturnsTrueIfMediaTypesDifferOnlyByCase()
        {
            MediaTypeHeaderValueEqualityComparer comparer = MediaTypeHeaderValueEqualityComparer.EqualityComparer;

            MediaTypeHeaderValue mediaType1 = new MediaTypeHeaderValue("text/xml");
            MediaTypeHeaderValue mediaType2 = new MediaTypeHeaderValue("TEXT/xml");
            Assert.Equal(mediaType1, mediaType2, comparer);

            mediaType1 = new MediaTypeHeaderValue("text/*");
            mediaType2 = new MediaTypeHeaderValue("TEXT/*");
            Assert.Equal(mediaType1, mediaType2, comparer);

            mediaType1 = new MediaTypeHeaderValue("*/*");
            mediaType2 = new MediaTypeHeaderValue("*/*");
            Assert.Equal(mediaType1, mediaType2, comparer);

            mediaType1 = new MediaTypeHeaderValue("text/*");
            mediaType1.CharSet = "someCharset";
            mediaType2 = new MediaTypeHeaderValue("TEXT/*");
            mediaType2.CharSet = "SOMECHARSET";
            Assert.Equal(mediaType1, mediaType2, comparer);

            mediaType1 = new MediaTypeHeaderValue("application/*");
            mediaType1.CharSet = "";
            mediaType2 = new MediaTypeHeaderValue("application/*");
            mediaType2.CharSet = null;
            Assert.Equal(mediaType1, mediaType2, comparer);
        }

        [Fact]
        [Trait("Description", "Equals(MediaTypeHeaderValue, MediaTypeHeaderValue) returns false if media types and charsets differ by more than case.")]
        public void EqualsReturnsFalseIfMediaTypesDifferByMoreThanCase()
        {
            MediaTypeHeaderValueEqualityComparer comparer = MediaTypeHeaderValueEqualityComparer.EqualityComparer;

            MediaTypeHeaderValue mediaType1 = new MediaTypeHeaderValue("text/xml");
            MediaTypeHeaderValue mediaType2 = new MediaTypeHeaderValue("TEST/xml");
            Assert.NotEqual(mediaType1, mediaType2, comparer);

            mediaType1 = new MediaTypeHeaderValue("text/*");
            mediaType1.CharSet = "someCharset";
            mediaType2 = new MediaTypeHeaderValue("TEXT/*");
            mediaType2.CharSet = "SOMEOTHERCHARSET";
            Assert.NotEqual(mediaType1, mediaType2, comparer);
        }

        [Theory]
        [TestDataSet(typeof(HttpUnitTestDataSets), "StandardMediaTypesWithQuality")]
        [Trait("Description", "Equals(MediaTypeHeaderValue, MediaTypeHeaderValue) returns true if media types differ only in quality.")]
        public void EqualsReturnsTrueIfMediaTypesDifferOnlyByQuality(MediaTypeWithQualityHeaderValue mediaType1)
        {
            MediaTypeHeaderValueEqualityComparer comparer = MediaTypeHeaderValueEqualityComparer.EqualityComparer;
            MediaTypeHeaderValue mediaType2 = new MediaTypeHeaderValue(mediaType1.MediaType);
            Assert.Equal(mediaType2, mediaType1, comparer);
        }
    }
}
