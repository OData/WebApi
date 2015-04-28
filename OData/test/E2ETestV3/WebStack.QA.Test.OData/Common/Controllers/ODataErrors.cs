using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData.Query;
using Microsoft.Data.OData;

namespace WebStack.QA.Test.OData.Common.Controllers
{
    /// <summary>
    /// A set of useful correctly formatted OData errors.
    /// </summary>
    public static class ODataErrors
    {
        public static HttpResponseException EntityNotFound(this HttpRequestMessage request)
        {
            return new HttpResponseException(
                    request.CreateResponse(
                        HttpStatusCode.NotFound,
                        new ODataError
                        {
                            Message = "The entity was not found.",
                            MessageLanguage = "en-US",
                            ErrorCode = "Entity Not Found.",
                            InnerError = new ODataInnerError(new ArgumentNullException("Test"))
                        }));
        }

        public static HttpResponseException DeletingLinkNotSupported(this HttpRequestMessage request, string navigation)
        {
            return new HttpResponseException(
                    request.CreateResponse(
                        HttpStatusCode.NotImplemented,
                        new ODataError
                        {
                            Message = string.Format("Deleting a '{0}' link is not supported.", navigation),
                            MessageLanguage = "en-US",
                            ErrorCode = "Deleting link failed."
                        }));
        }

        public static HttpResponseException CreatingLinkNotSupported(this HttpRequestMessage request, string navigation)
        {
            return new HttpResponseException(
                    request.CreateResponse(
                        HttpStatusCode.NotImplemented,
                        new ODataError
                        {
                            Message = string.Format("Creating a '{0}' link is not supported.", navigation),
                            MessageLanguage = "en-US",
                            ErrorCode = "Creating link failed."
                        }));
        }

        /// <summary>
        /// An example of how you might fail if the request's query options includes something you can't support
        /// </summary>
        public static void AssertNoFilter(this HttpRequestMessage request, ODataQueryOptions options)
        {
            if (!string.IsNullOrEmpty(options.RawValues.Filter))
            {
                throw new HttpResponseException(
                    request.CreateResponse(
                        HttpStatusCode.NotImplemented,
                        new ODataError
                        {
                            Message = "The Products entityset does not support $filter.",
                            MessageLanguage = "en-US",
                            ErrorCode = "$filter is not supported."
                        }));
            }
        }
    }
}
