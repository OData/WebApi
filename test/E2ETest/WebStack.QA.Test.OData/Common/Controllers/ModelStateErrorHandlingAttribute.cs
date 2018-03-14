using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;

namespace WebStack.QA.Test.OData.Common.Controllers
{
    public class ModelStateErrorHandlingAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            if (!actionContext.ModelState.IsValid)
            {
                throw new HttpResponseException(
                    actionContext.Request.CreateErrorResponse(
                        System.Net.HttpStatusCode.BadRequest,
                        actionContext.ModelState));
            }
            base.OnActionExecuting(actionContext);
        }
    }
}
