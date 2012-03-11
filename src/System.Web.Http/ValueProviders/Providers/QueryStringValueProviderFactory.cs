using System.Globalization;
using System.Web.Http.Controllers;

namespace System.Web.Http.ValueProviders.Providers
{
    public class QueryStringValueProviderFactory : ValueProviderFactory, IUriValueProviderFactory
    {
        public override IValueProvider GetValueProvider(HttpActionContext actionContext)
        {
            return new QueryStringValueProvider(actionContext, CultureInfo.CurrentCulture);
        }
    }
}
