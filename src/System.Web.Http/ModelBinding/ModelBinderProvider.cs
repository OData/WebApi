using System.Web.Http.Controllers;

namespace System.Web.Http.ModelBinding
{
    public abstract class ModelBinderProvider
    {
        public abstract IModelBinder GetBinder(HttpActionContext actionContext, ModelBindingContext bindingContext);
    }
}
