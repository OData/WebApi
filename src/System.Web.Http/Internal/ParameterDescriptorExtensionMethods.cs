using System.Diagnostics.Contracts;
using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;

namespace System.Web.Http.Internal
{
    internal static class ParameterDescriptorExtensionMethods
    {
        public static bool IsStructuredBodyParameter(this HttpParameterDescriptor parameterDescriptor)
        {
            Contract.Assert(parameterDescriptor != null, "parameterDescriptor cannot be null.");

            return TypeHelper.IsStructuredBodyContentType(parameterDescriptor.ParameterType);
        }
    }
}
