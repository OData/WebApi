using System.Web.Http.Controllers;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// Interface for model binding.
    /// </summary>
    public interface IModelBinder
    {
        bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext);
    }
}
