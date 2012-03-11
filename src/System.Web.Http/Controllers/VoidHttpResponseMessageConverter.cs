using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.Controllers
{
    internal sealed class VoidHttpResponseMessageConverter : ActionResponseConverter
    {
        public override Task<HttpResponseMessage> Convert(HttpControllerContext controllerContext, object responseValue, CancellationToken cancellation)
        {
            // responseValue is ignored because the Action returns void. 
            return TaskHelpers.RunSynchronously(() => ConvertSync(controllerContext), cancellation);
        }

        private static HttpResponseMessage ConvertSync(HttpControllerContext controllerContext)
        {
            return controllerContext.Request.CreateResponse();
        }
    }
}
