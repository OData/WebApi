using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace System.Web.Http.Internal
{
    internal static class TypeDescriptorHelper
    {
        internal static ICustomTypeDescriptor Get(Type type)
        {
            return new AssociatedMetadataTypeTypeDescriptionProvider(type).GetTypeDescriptor(type);
        }
    }
}
