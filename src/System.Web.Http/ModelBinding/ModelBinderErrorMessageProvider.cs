using System.Web.Http.Controllers;
using System.Web.Http.Metadata;

namespace System.Web.Http.ModelBinding
{
    public delegate string ModelBinderErrorMessageProvider(HttpActionContext actionContext, ModelMetadata modelMetadata, object incomingValue);
}
