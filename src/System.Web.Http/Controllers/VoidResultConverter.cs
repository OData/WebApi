using System.Diagnostics.Contracts;
using System.Net.Http;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// A converter for creating a response from actions that do not return a value.
    /// </summary>
    public class VoidResultConverter : IActionResultConverter
    {
        public HttpResponseMessage Convert(HttpControllerContext controllerContext, object actionResult)
        {
            Contract.Assert(actionResult == null);
            return controllerContext.Request.CreateResponse();
        }
    }
}
