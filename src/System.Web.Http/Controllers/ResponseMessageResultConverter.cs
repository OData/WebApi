using System.Net.Http;
using System.Web.Http.Properties;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// A converter for actions with a return type of <see cref="HttpResponseMessage"/>.
    /// </summary>
    public class ResponseMessageResultConverter : IActionResultConverter
    {
        public HttpResponseMessage Convert(HttpControllerContext controllerContext, object actionResult)
        {
            HttpResponseMessage response = (HttpResponseMessage)actionResult;
            if (response == null)
            {
                throw Error.InvalidOperation(SRResources.ResponseMessageResultConverter_NullHttpResponseMessage);
            }

            response.EnsureResponseHasRequest(controllerContext.Request);
            return response;
        }
    }
}
