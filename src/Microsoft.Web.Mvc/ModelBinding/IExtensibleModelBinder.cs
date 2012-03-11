using System.Web.Mvc;

namespace Microsoft.Web.Mvc.ModelBinding
{
    public interface IExtensibleModelBinder
    {
        bool BindModel(ControllerContext controllerContext, ExtensibleModelBindingContext bindingContext);
    }
}
