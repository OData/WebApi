using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.Controllers
{
    // Used when the action's return value is already a HttpResponseMessage
    internal sealed class HttpResponseMessageConverter : ActionResponseConverter
    {
        public override Task<HttpResponseMessage> Convert(HttpControllerContext controllerContext, object responseValue, CancellationToken cancellation)
        {
            Contract.Assert(controllerContext != null);
            return TaskHelpers.RunSynchronously(() => ConvertSync(controllerContext, (HttpResponseMessage)responseValue), cancellation);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "caller becomes owner.")]
        internal static HttpResponseMessage ConvertSync(HttpControllerContext controllerContext, HttpResponseMessage responseValue)
        {
            Contract.Assert(controllerContext != null);

            responseValue.RequestMessage = controllerContext.Request;
            return responseValue;
        }
    }

    // Used when converting a TResponseValue, which could be an arbitrary type for which we
    // don't have a more specific converter (eg, it's not HttpContent, HttpRequestMessage).
    internal sealed class HttpResponseMessageConverter<TResponseValue> : ActionResponseConverter
    {
        public override Task<HttpResponseMessage> Convert(HttpControllerContext controllerContext, object responseValue, CancellationToken cancellation)
        {
            Contract.Assert(controllerContext != null);
            return TaskHelpers.RunSynchronously(() => ConvertSync(controllerContext, responseValue), cancellation);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "caller becomes owner.")]
        private static HttpResponseMessage ConvertSync(HttpControllerContext controllerContext, object responseValue)
        {
            HttpRequestMessage request = controllerContext.Request;

            HttpContent responseAsContent = responseValue as HttpContent;
            if (responseAsContent != null)
            {
                var resp = request.CreateResponse();
                resp.Content = responseAsContent;
                return resp;
            }

            HttpConfiguration config = controllerContext.Configuration;
            HttpResponseMessage response = request.CreateResponse<TResponseValue>(HttpStatusCode.OK, (TResponseValue)responseValue, config);
            return response;
        }
    }
}
