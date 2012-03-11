using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace Microsoft.Web.Http.Data
{
    public sealed class DataControllerActionInvoker : ApiControllerActionInvoker
    {
        public override Task<HttpResponseMessage> InvokeActionAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            DataController controller = (DataController)actionContext.ControllerContext.Controller;
            controller.ActionContext = actionContext;
            return base.InvokeActionAsync(actionContext, cancellationToken);
        }
    }
}
