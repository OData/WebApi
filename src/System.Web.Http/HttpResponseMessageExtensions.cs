using System.ComponentModel;
using System.Net.Http;
using System.Web.Http.Common;

namespace System.Web.Http
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpResponseMessageExtensions
    {
        public static bool TryGetObjectValue<T>(this HttpResponseMessage response, out T value) where T : class
        {
            if (response == null)
            {
                throw Error.ArgumentNull("response");
            }

            ObjectContent content = response.Content as ObjectContent;
            if (content != null)
            {
                value = content.Value as T;
                return value != null;
            }

            value = null;
            return false;
        }

        public static bool TrySetObjectValue<T>(this HttpResponseMessage response, T value) where T : class
        {
            if (response == null)
            {
                throw Error.ArgumentNull("response");
            }

            ObjectContent content = response.Content as ObjectContent;
            if (content != null)
            {
                try
                {
                    content.Value = value;
                }
                catch (ArgumentException)
                {
                    return false;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }

                return true;
            }

            value = null;
            return false;
        }
    }
}
