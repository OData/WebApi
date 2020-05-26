using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Interfaces;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    internal static class RequestMethodExtensions
    {
        /// <summary>
        /// Returns the request method and in the case of Options request it returns the Access-Control-Request-Method present in the
        /// preflight request for the request method that will be used for the actual request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        internal static ODataRequestMethod GetRequestMethodOrPreflightMethod(this IWebApiRequestMessage request)
        {
            if (request.Method == ODataRequestMethod.Options)
            {
                IEnumerable<string> values;
                if (request.Headers.TryGetValues("Access-Control-Request-Method", out values))
                {
                    return ConvertToODataRequestMethod(values.FirstOrDefault());
                }
                else
                {
                    return ODataRequestMethod.Unknown;
                }
            }

            return request.Method;
        }
        /// <summary>
        /// Converts string request method to ODataRequestMethod
        /// </summary>
        /// <param name="requestMethod"></param>
        /// <returns></returns>
        private static ODataRequestMethod ConvertToODataRequestMethod(string requestMethod)
        {
            if (requestMethod == null)
            {
                return ODataRequestMethod.Unknown;
            }

            switch (requestMethod.ToUpperInvariant())
            {
                case "GET": return ODataRequestMethod.Get;
                case "POST": return ODataRequestMethod.Post;
                case "PUT": return ODataRequestMethod.Put;
                case "PATCH": return ODataRequestMethod.Patch;
                case "DELETE": return ODataRequestMethod.Delete;
                case "MERGE": return ODataRequestMethod.Merge;
                default: return ODataRequestMethod.Unknown;
            }
        }
    }
}
