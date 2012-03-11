using System.Threading;

namespace System.Web.Mvc
{
    public class CancellationTokenModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            return default(CancellationToken);
        }
    }
}
