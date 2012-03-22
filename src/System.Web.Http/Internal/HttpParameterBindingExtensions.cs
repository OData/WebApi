using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;

namespace System.Web.Http.Internal
{
    internal static class HttpParameterBindingExtensions
    {
        public static bool WillReadUri(this HttpParameterBinding parameterBinding)
        {
            if (parameterBinding == null)
            {
                throw Error.ArgumentNull("parameterBinding");
            }

            ModelBinderParameterBinding modelParameterBinding = parameterBinding as ModelBinderParameterBinding;
            if (modelParameterBinding != null)
            {
                if (modelParameterBinding.ValueProviderFactories.All(factory => factory is IUriValueProviderFactory))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasDefaultValue(this HttpParameterBinding parameterBinding)
        {
            if (parameterBinding == null)
            {
                throw Error.ArgumentNull("parameterBinding");
            }

            return parameterBinding.Descriptor.DefaultValue != null;
        }
    }
}
