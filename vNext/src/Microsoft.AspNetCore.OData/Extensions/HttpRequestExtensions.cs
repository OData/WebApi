using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Formatter;

namespace Microsoft.AspNetCore.OData.Extensions
{
    public static class HttpRequestExtensions
    {
        public static IODataFeature ODataFeature(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.HttpContext.ODataFeature();
        }

        public static IETagHandler ETagHandler(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.HttpContext.ETagHandler();
        }

        public static IAssemblyProvider AssemblyProvider(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.HttpContext.AssemblyProvider();
        }

        public static bool HasQueryOptions(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request?.Query != null && request.Query.Count > 0;
        }
    }
}