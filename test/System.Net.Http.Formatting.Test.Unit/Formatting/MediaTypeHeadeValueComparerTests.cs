using System.Net.Http.Headers;
using Xunit;

namespace System.Net.Http.Formatting
{
    public class MediaTypeHeadeValueComparerTests
    {

        [Fact]
        [Trait("Description", "MediaTypeHeadeValueComparer.Comparer returns same MediaTypeHeadeValueComparer instance each time.")]
        public void Comparer_Returns_MediaTypeHeadeValueComparer()
        {
            MediaTypeHeaderValueComparer comparer1 = MediaTypeHeaderValueComparer.Comparer;
            MediaTypeHeaderValueComparer comparer2 = MediaTypeHeaderValueComparer.Comparer;

            Assert.NotNull(comparer1);
            Assert.Same(comparer1, comparer2);
        }

        [Fact]
        [Trait("Description", "MediaTypeHeadeValueComparer.Compare returns 0 for same MediaTypeHeaderValue instance.")]
        public void Compare_Returns_0_For_Same_MediaTypeHeaderValue()
        {
            MediaTypeHeaderValueComparer comparer = MediaTypeHeaderValueComparer.Comparer;
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("text/xml");

            Assert.Equal(0, comparer.Compare(mediaType, mediaType));
        }

        [Fact]
        [Trait("Description", "MediaTypeHeadeValueComparer.Compare returns 0 for MediaTypeHeaderValue instances that differ only by case.")]
        public void Compare_Returns_0_For_MediaTypeHeaderValues_Differing_Only_By_Case()
        {
            MediaTypeHeaderValueComparer comparer = MediaTypeHeaderValueComparer.Comparer;

            MediaTypeHeaderValue mediaType1 = new MediaTypeHeaderValue("text/Xml");
            MediaTypeHeaderValue mediaType2 = new MediaTypeHeaderValue("texT/xml");
            Assert.Equal(0, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeHeaderValue("application/*");
            mediaType2 = new MediaTypeHeaderValue("APPLICATION/*");
            Assert.Equal(0, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeHeaderValue("*/*");
            mediaType2 = new MediaTypeHeaderValue("*/*");
            Assert.Equal(0, comparer.Compare(mediaType1, mediaType2));
        }

        [Fact]
        [Trait("Description", "MediaTypeHeadeValueComparer.Compare returns 0 for MediaTypeHeaderValue instances that differ by non-q-value parameters.")]
        public void Compare_Returns_0_For_MediaTypeHeaderValues_Differ_By_Non_Q_Parameters()
        {
            MediaTypeHeaderValueComparer comparer = MediaTypeHeaderValueComparer.Comparer;

            MediaTypeHeaderValue mediaType1 = new MediaTypeHeaderValue("*/*");
            mediaType1.CharSet = "someCharset";
            mediaType1.Parameters.Add(new NameValueHeaderValue("someName", "someValue"));
            MediaTypeHeaderValue mediaType2 = new MediaTypeHeaderValue("*/*");
            mediaType2.CharSet = "someOtherCharset";
            mediaType2.Parameters.Add(new NameValueHeaderValue("someName", "someOtherValue"));
            Assert.Equal(0, comparer.Compare(mediaType1, mediaType2));
        }

        [Fact]
        [Trait("Description", "MediaTypeHeadeValueComparer.Compare returns 0 for MediaTypeHeaderValue with the same Q when the Media types are not media ranges or are the same media ranges.")]
        public void Compare_Returns_0_For_MediaTypeHeaderValues_With_Same_Q_Value()
        {
            MediaTypeHeaderValueComparer comparer = MediaTypeHeaderValueComparer.Comparer;

            MediaTypeWithQualityHeaderValue mediaType1 = new MediaTypeWithQualityHeaderValue("text/xml", 0.5);
            MediaTypeWithQualityHeaderValue mediaType2 = new MediaTypeWithQualityHeaderValue("text/xml", 0.50);
            Assert.Equal(0, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeWithQualityHeaderValue("text/xml", .7);
            mediaType2 = new MediaTypeWithQualityHeaderValue("application/xml", .7);
            Assert.Equal(0, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeWithQualityHeaderValue("text/xml");
            mediaType2 = new MediaTypeWithQualityHeaderValue("text/xml");
            Assert.Equal(0, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeWithQualityHeaderValue("text/xml");
            mediaType2 = new MediaTypeWithQualityHeaderValue("text/plain");
            Assert.Equal(0, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeWithQualityHeaderValue("text/*", 0.3);
            mediaType2 = new MediaTypeWithQualityHeaderValue("text/*", .3);
            Assert.Equal(0, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeWithQualityHeaderValue("*/*");
            mediaType2 = new MediaTypeWithQualityHeaderValue("*/*");
            Assert.Equal(0, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeWithQualityHeaderValue("text/*", .1);
            mediaType2 = new MediaTypeWithQualityHeaderValue("application/xml", .1);
            Assert.Equal(0, comparer.Compare(mediaType1, mediaType2));
        }

        [Fact]
        [Trait("Description", "MediaTypeHeadeValueComparer.Compare returns 1 if the first parameter has a smaller Q value.")]
        public void Compare_Returns_1_If_MediaType1_Has_Smaller_Q_Value()
        {
            MediaTypeHeaderValueComparer comparer = MediaTypeHeaderValueComparer.Comparer;

            MediaTypeWithQualityHeaderValue mediaType1 = new MediaTypeWithQualityHeaderValue("text/xml", 0.49);
            MediaTypeWithQualityHeaderValue mediaType2 = new MediaTypeWithQualityHeaderValue("text/xml", 0.50);
            Assert.Equal(1, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeWithQualityHeaderValue("text/xml", .0);
            mediaType2 = new MediaTypeWithQualityHeaderValue("application/xml", .7);
            Assert.Equal(1, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeWithQualityHeaderValue("text/xml", 0.9);
            mediaType2 = new MediaTypeWithQualityHeaderValue("text/xml");
            Assert.Equal(1, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeWithQualityHeaderValue("text/xml", 0);
            mediaType2 = new MediaTypeWithQualityHeaderValue("text/plain", 0.1);
            Assert.Equal(1, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeWithQualityHeaderValue("*/*", 0.3);
            mediaType2 = new MediaTypeWithQualityHeaderValue("text/*", .31);
            Assert.Equal(1, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeWithQualityHeaderValue("text/*", 0.5);
            mediaType2 = new MediaTypeWithQualityHeaderValue("*/*", 0.6);
            Assert.Equal(1, comparer.Compare(mediaType1, mediaType2));
        }

        [Fact]
        [Trait("Description", "MediaTypeHeadeValueComparer.Compare returns 1 if the Q values are the same but the first parameter is a less specific media range.")]
        public void Compare_Returns_1_If_Q_Value_Is_Same_But_MediaType1_Is_Media_Range()
        {
            MediaTypeHeaderValueComparer comparer = MediaTypeHeaderValueComparer.Comparer;

            MediaTypeWithQualityHeaderValue mediaType1 = new MediaTypeWithQualityHeaderValue("text/*", 0.50);
            MediaTypeWithQualityHeaderValue mediaType2 = new MediaTypeWithQualityHeaderValue("text/xml", 0.50);
            Assert.Equal(1, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeWithQualityHeaderValue("*/*");
            mediaType2 = new MediaTypeWithQualityHeaderValue("text/xml");
            Assert.Equal(1, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeWithQualityHeaderValue("*/*", 0.2);
            mediaType2 = new MediaTypeWithQualityHeaderValue("text/*", 0.2);
            Assert.Equal(1, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeWithQualityHeaderValue("application/json", 0.2);
            mediaType2 = new MediaTypeWithQualityHeaderValue("application/json", 0.2);
            mediaType2.CharSet = "someCharSet";
            Assert.Equal(1, comparer.Compare(mediaType1, mediaType2));
        }

        [Fact]
        [Trait("Description", "MediaTypeHeadeValueComparer.Compare returns -1 if the first parameter has a larger Q value.")]
        public void Compare_Returns_Negative_1_If_MediaType1_Has_Larger_Q_Value()
        {
            MediaTypeHeaderValueComparer comparer = MediaTypeHeaderValueComparer.Comparer;

            MediaTypeWithQualityHeaderValue mediaType1 = new MediaTypeWithQualityHeaderValue("text/xml", 0.51);
            MediaTypeWithQualityHeaderValue mediaType2 = new MediaTypeWithQualityHeaderValue("text/xml", 0.50);
            Assert.Equal(-1, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeWithQualityHeaderValue("text/xml", .7);
            mediaType2 = new MediaTypeWithQualityHeaderValue("application/xml", .0);
            Assert.Equal(-1, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeWithQualityHeaderValue("text/xml");
            mediaType2 = new MediaTypeWithQualityHeaderValue("text/xml", 0.9);
            Assert.Equal(-1, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeWithQualityHeaderValue("text/xml", 0.1);
            mediaType2 = new MediaTypeWithQualityHeaderValue("text/plain", 0);
            Assert.Equal(-1, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeWithQualityHeaderValue("*/*", 0.31);
            mediaType2 = new MediaTypeWithQualityHeaderValue("text/*", .30);
            Assert.Equal(-1, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeWithQualityHeaderValue("text/*", 0.6);
            mediaType2 = new MediaTypeWithQualityHeaderValue("*/*", 0.5);
            Assert.Equal(-1, comparer.Compare(mediaType1, mediaType2));
        }

        [Fact]
        [Trait("Description", "MediaTypeHeadeValueComparer.Compare returns negative 1 if the Q values are the same but the second parameter is a less specific media range.")]
        public void Compare_Returns_Negative_1_If_Q_Value_Is_Same_But_MediaType2_Is_Media_Range()
        {
            MediaTypeHeaderValueComparer comparer = MediaTypeHeaderValueComparer.Comparer;

            MediaTypeWithQualityHeaderValue mediaType1 = new MediaTypeWithQualityHeaderValue("text/xml", 0.50);
            MediaTypeWithQualityHeaderValue mediaType2 = new MediaTypeWithQualityHeaderValue("text/*", 0.50);
            Assert.Equal(-1, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeWithQualityHeaderValue("x/y");
            mediaType2 = new MediaTypeWithQualityHeaderValue("*/*");
            Assert.Equal(-1, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeWithQualityHeaderValue("application/*", 0.2);
            mediaType2 = new MediaTypeWithQualityHeaderValue("*/*", 0.2);
            Assert.Equal(-1, comparer.Compare(mediaType1, mediaType2));

            mediaType1 = new MediaTypeWithQualityHeaderValue("application/json", 0.2);
            mediaType1.CharSet = "someCharSet";
            mediaType2 = new MediaTypeWithQualityHeaderValue("application/json", 0.2);
            Assert.Equal(-1, comparer.Compare(mediaType1, mediaType2));
        }
    }
}
