using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace Xunit
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpResponseMessageAssertExtensions
    {
        public static void MustContain<T>(this HttpResponseMessage responseMessage,
                                               HttpStatusCode expectedStatusCode,
                                               string expectedContentMediaType,
                                               T expectedContent)
        {
            MustContainHelper(responseMessage, expectedStatusCode, expectedContentMediaType, expectedContent, new MediaTypeFormatterCollection());
        }

        public static void MustContain<T>(this HttpResponseMessage responseMessage,
                                               HttpStatusCode expectedStatusCode,
                                               string expectedContentMediaType,
                                               T expectedContent,
                                               IEnumerable<MediaTypeFormatter> formatters)
        {
            MustContainHelper(responseMessage, expectedStatusCode, expectedContentMediaType, expectedContent, formatters);
        }

        private static void MustContainHelper<T>(HttpResponseMessage responseMessage,
                                                 HttpStatusCode expectedStatusCode,
                                                 string expectedContentMediaType,
                                                 T expectedContent,
                                                 IEnumerable<MediaTypeFormatter> formatters)
        {
            Assert.Equal(expectedStatusCode, responseMessage.StatusCode);
            Assert.NotNull(responseMessage.Content);
            Assert.NotNull(responseMessage.Content.Headers.ContentType);
            Assert.Equal(MediaTypeHeaderValue.Parse(expectedContentMediaType).ToString(), responseMessage.Content.Headers.ContentType.ToString());
            Assert.Equal(expectedContent, responseMessage.Content.ReadAsAsync<T>(formatters).Result);
        }
    }
}
