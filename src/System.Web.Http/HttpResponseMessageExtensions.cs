using System.ComponentModel;
using System.Net.Http;

namespace System.Web.Http
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class HttpResponseMessageExtensions
    {
        internal static bool TryGetObjectValue<T>(this HttpResponseMessage response, out T value) where T : class
        {
            if (response != null)
            {
                ObjectContent content = response.Content as ObjectContent;
                if (content != null)
                {
                    value = content.Value as T;
                    return value != null;
                }
            }

            value = default(T);
            return false;
        }
    }
}
