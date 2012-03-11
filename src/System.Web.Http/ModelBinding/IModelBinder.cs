using System.Web.Http.Controllers;

namespace System.Web.Http.ModelBinding
{
    public interface IModelBinder
    {
        bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext);
    }
}
