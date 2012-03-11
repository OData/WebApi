using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    public abstract class ModelBinderProvider
    {
        public abstract IExtensibleModelBinder GetBinder(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext);
    }
}
