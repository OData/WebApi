using System.Web.Http.Controllers;

namespace System.Web.Http.ModelBinding
{
    // Interface for model binding
    public interface IModelBinder
    {
        bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext);
    }
}
