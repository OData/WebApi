using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Infrastructure;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.OData.Extensions
{
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Gets the <see cref="HttpRequestProperties"/> instance containing OData methods and properties
        /// for given <see cref="HttpRequest"/>.
        /// </summary>
        /// <param name="request">The request of interest.</param>
        /// <returns>
        /// An object through which OData methods and properties for given <paramref name="request"/> are available.
        /// </returns>
        public static ODataProperties ODataProperties(this HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return request.HttpContext.RequestServices.GetRequiredService<ODataProperties>();
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