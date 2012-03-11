using System.Web.Http.Controllers;

namespace System.Web.Http.ValueProviders
{
    public abstract class ValueProviderFactory
    {
        public abstract IValueProvider GetValueProvider(HttpActionContext actionContext);
    }
}
