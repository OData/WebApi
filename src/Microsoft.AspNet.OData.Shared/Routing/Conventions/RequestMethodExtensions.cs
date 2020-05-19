using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Interfaces;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    internal static class RequestMethodExtensions 
    {
        internal static ODataRequestMethod GetRequestMethodOrPreflightMethod(this IWebApiRequestMessage request)
        {
            if (request.Method == ODataRequestMethod.Options)
            {
                IEnumerable<string> values;
                request.Headers.TryGetValues("Access-Control-Request-Method", out values);
                var enumerable = values.ToList();
                if (enumerable.Count()!=0)
                {
                    return ToOdataRequestMethod(enumerable.First());
                }

                return ODataRequestMethod.Unknown;
            }

            return request.Method;
        }

        private static ODataRequestMethod ToOdataRequestMethod(string requestMethod)
        {
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
