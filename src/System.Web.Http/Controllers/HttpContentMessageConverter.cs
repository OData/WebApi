using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.Controllers
{
    // Used when converting a HttpContent.
    internal sealed class HttpContentMessageConverter : ActionResponseConverter
    {
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "caller becomes owner.")]
        public override Task<HttpResponseMessage> Convert(HttpControllerContext controllerContext, object responseValue, CancellationToken cancellation)
        {
            Contract.Assert(controllerContext != null);
            return TaskHelpers.RunSynchronously(() => ConvertSync(controllerContext, (HttpContent)responseValue), cancellation);
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "caller becomes owner.")]
        internal static HttpResponseMessage ConvertSync(HttpControllerContext controllerContext, HttpContent responseValue)
        {
            Contract.Assert(controllerContext != null);

            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = responseValue;

            // Wrap the content in a Response and chain.
            return HttpResponseMessageConverter.ConvertSync(controllerContext, response);
        }
    }
}
