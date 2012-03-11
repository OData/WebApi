using System.Globalization;
using System.Web.Http.Controllers;

namespace System.Web.Http.ValueProviders.Providers
{
    public class RouteDataValueProviderFactory : ValueProviderFactory, IUriValueProviderFactory
    {
        public RouteDataValueProviderFactory()
        {
        }

        public override IValueProvider GetValueProvider(HttpActionContext actionContext)
        {
            return new RouteDataValueProvider(actionContext, CultureInfo.InvariantCulture);
        }
    }
}
